using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

using AutoMapper;

using STR.Common.Extensions;
using STR.Common.Messages;

using STR.MvvmCommon.Contracts;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Wpf.Extensions;
using TcpMonitor.Wpf.ViewEntities;
using TcpMonitor.Wpf.ViewModels;


namespace TcpMonitor.Wpf.Controllers {

  [Export(typeof(IController))]
  public sealed class ConnectionsController : IController {

    #region Private Fields

    private readonly TimeSpan highlightLagTime = TimeSpan.FromSeconds(1);
    private readonly TimeSpan    closedLagTime = TimeSpan.FromSeconds(1);

    private readonly Object entityLock = new Object();
    private readonly Object packetLock = new Object();

    private readonly List<DomainConnection> connections;
    private readonly List<DomainConnection> packets;

    private readonly DispatcherTimer connectionsTimer;
    private readonly DispatcherTimer     displayTimer;
    private readonly DispatcherTimer      memoryTimer;

    private readonly ConnectionsViewModel viewModel;

    private readonly ConnectionViewEntityComparer comparer;

    private readonly IMapper    mapper;
    private readonly IMessenger messenger;

    private readonly IConnectionsService connectionService;
    private readonly ICapturePackets     capturePackets;

    #endregion Private Fields

    #region Constructor

    [ImportingConstructor]
    public ConnectionsController(ConnectionsViewModel ViewModel, IMapper Mapper, IMessenger Messenger, IConnectionsService ConnectionService, ICapturePackets CapturePackets) {
      viewModel = ViewModel;

      viewModel.Connections = new ObservableCollection<ConnectionViewEntity>();

      mapper    = Mapper;
      messenger = Messenger;

      connectionService = ConnectionService;
      capturePackets    = CapturePackets;

      connections = new List<DomainConnection>();
      packets     = new List<DomainConnection>();

      connectionsTimer = new DispatcherTimer();
          displayTimer = new DispatcherTimer();
           memoryTimer = new DispatcherTimer();

      comparer = new ConnectionViewEntityComparer();
    }

    #endregion Constructor

    #region IController Implementation

    public int InitializePriority { get; } = 100;

    public async Task InitializeAsync() {
      connectionsTimer.Tick    += onConnectionsTimerTick;
      connectionsTimer.Interval = TimeSpan.FromMilliseconds(20);

      connectionsTimer.Start();

      displayTimer.Tick    += onDisplayTimerTick;
      displayTimer.Interval = TimeSpan.FromMilliseconds(20);

      displayTimer.Start();

      memoryTimer.Tick    += onMemoryTimerTick;
      memoryTimer.Interval = TimeSpan.FromSeconds(1);

      memoryTimer.Start();

      registerMessages();

      await Task.CompletedTask;
    }

    #endregion IController Implementation

    #region Messages

    private void registerMessages() {
      messenger.Register<ApplicationLoadedMessage>(this, onApplicationLoaded);

      messenger.Register<ApplicationClosingMessage>(this, onApplicationClosing);
    }

    private void onApplicationLoaded(ApplicationLoadedMessage message) {
      capturePackets.RegisterPacketCapture(onPacketCaptured);
    }

    private void onApplicationClosing(ApplicationClosingMessage message) {
      capturePackets.UnregisterPacketCapture();
    }

    #endregion Messages

    #region Private Methods

    private void onPacketCaptured(DomainPacket packet) {
      List<ConnectionViewEntity> locals;

      lock(entityLock) locals = viewModel.Connections.Where(c => c.Key == packet.Key1).ToList();

      locals.ForEach(local => {
        local.PacketsSent = (local.PacketsSent ?? 0) + 1;
        local.BytesSent   = (local.BytesSent   ?? 0) + packet.Bytes;

        local.HasData  = true;
        local.IsNew    = false;
        local.IsClosed = false;

        local.LastChange = DateTime.Now;
      });

      List<ConnectionViewEntity> remotes;

      lock(entityLock) remotes = viewModel.Connections.Where(c => c.Key == packet.Key2).ToList();

      remotes.ForEach(remote => {
        remote.PacketsReceived = (remote.PacketsReceived ?? 0) + 1;
        remote.BytesReceived   = (remote.BytesReceived   ?? 0) + packet.Bytes;

        remote.HasData  = true;
        remote.IsNew    = false;
        remote.IsClosed = false;

        remote.LastChange = DateTime.Now;
      });

      if (!locals.Any() && !remotes.Any()) {
        viewModel.DroppedPackets++;

        if (!viewModel.ViewDropped) return;

        bool exists;

        lock(packetLock) exists = packets.Any(p => packet.Key1 == p.Key || packet.Key2 == p.Key);

        if (!exists) {
          lock(packetLock) {
            if (!packets.Any(p => packet.Key1 == p.Key || packet.Key2 == p.Key)) {
              bool isSourceLocal      = connectionService.IsLocalAddress(packet.SourceEndPoint);
              bool isDestinationLocal = connectionService.IsLocalAddress(packet.DestinationEndPoint);

              DomainConnection connection = mapper.Map<DomainConnection>(packet);

              if ((isSourceLocal && isDestinationLocal) || isSourceLocal) {
                connection.Key = packet.Key1;

                connection.LocalEndPoint  = packet.SourceEndPoint;
                connection.RemoteEndPoint = packet.DestinationEndPoint;
              }
              else {
                connection.Key = packet.Key2;

                connection.LocalEndPoint  = packet.DestinationEndPoint;
                connection.RemoteEndPoint = packet.SourceEndPoint;
              }

              connection.ResolveHostNames(connectionService).FireAndForget();

              packets.Add(connection);
            }
          }
        }
      }
    }

    private async void onConnectionsTimerTick(object sender, EventArgs args) {
      connectionsTimer.Stop();

      Stopwatch watch = Stopwatch.StartNew();

      List<DomainConnection> incoming = await connectionService.GetConnectionsAsync();

      if (!viewModel.ViewPidZero) incoming.Where(c => c.Pid == 0).ToList().ForEach(c => incoming.Remove(c));

      if (viewModel.ViewDropped) {
        lock(packetLock) {
          packets.Where(p => p.Pid != -1).ToList().ForEach(p => { packets.Remove(p); });

          incoming.AddRange(packets);
        }
      }

      var mods = (from row1 in incoming
                  join row2 in connections on row1.Key equals row2.Key
                select new { Mod = row1, Match = row2 }).ToList();

      mods.ForEach(set => mapper.Map(set.Mod, set.Match));

      List<DomainConnection> adds = (from row1 in incoming
                                     join row2 in connections on row1.Key equals row2.Key into collGroup
                                     from sub  in collGroup.DefaultIfEmpty()
                                    where sub == null
                                   select row1).ToList();

      adds.ForEach(add => add.ResolveHostNames(connectionService).FireAndForget());

      connections.AddRange(adds);

      List<DomainConnection> dels = (from row1 in connections
                                     join row2 in incoming on row1.Key equals row2.Key into collGroup
                                     from sub  in collGroup.DefaultIfEmpty()
                                    where sub == null
                                   select row1).ToList();

      dels.ForEach(del => connections.Remove(del));

      viewModel.ConnectionsPass = watch.ElapsedMilliseconds;

      connectionsTimer.Start();
    }

    private void onDisplayTimerTick(object sender, EventArgs args) {
      displayTimer.Stop();

      Stopwatch watch = Stopwatch.StartNew();

      DateTime now = DateTime.Now;

      viewModel.Connections.Where(c => now - c.LastChange > highlightLagTime).ForEach(c => {
        c.IsNew      = false;
        c.HasChanged = false;
        c.HasData    = false;
      });

      List<ConnectionViewEntity> closed = viewModel.Connections.Where(c => now - c.LastChange > closedLagTime && c.IsClosed).ToList();

      lock(entityLock) closed.ForEach(c => viewModel.Connections.Remove(c));

      var mods = (from row1 in connections
                  join row2 in viewModel.Connections on row1.Key equals row2.Key
                select new { Mod = row1, Match = row2 }).ToList();

      mods.ForEach(set => {
        if (set.Mod.Pid > 0 && set.Match.Pid > 0 && set.Mod.Pid != set.Match.Pid) return;

        if ((set.Mod.Pid > 0 && set.Match.Pid < 1) || set.Mod.ProcessName != set.Match.ProcessName || set.Mod.State != set.Match.State || set.Mod.LocalHostName != set.Match.LocalHostName || set.Mod.RemoteHostName != set.Match.RemoteHostName) {
          set.Match.HasChanged = set.Mod.State != set.Match.State;

          mapper.Map(set.Mod, set.Match);
        }
      });

      List<DomainConnection> adds = (from row1 in connections
                                     join row2 in viewModel.Connections on row1.Key equals row2.Key into collGroup
                                     from sub  in collGroup.DefaultIfEmpty()
                                    where sub == null
                                   select row1).ToList();

      List<ConnectionViewEntity> added = mapper.Map<List<ConnectionViewEntity>>(adds.Where(add => !String.IsNullOrEmpty(add.ProcessName)));

      added.ForEach(add => add.IsNew = true);

      lock(entityLock) viewModel.Connections.OrderedMerge(added.OrderBy(add => add, comparer));

      List<ConnectionViewEntity> dels = (from row1 in viewModel.Connections
                                         join row2 in connections on row1.Key equals row2.Key into collGroup
                                         from sub  in collGroup.DefaultIfEmpty()
                                        where sub == null
                                           && !row1.IsClosed
                                       select row1).ToList();

      dels.ForEach(del => { del.IsClosed = true; del.LastChange = DateTime.Now; });

      lock(entityLock) viewModel.Connections.Sort(comparer);

      if (viewModel.IsFiltered && !String.IsNullOrEmpty(viewModel.ConnectionFilter)) {
        try {
          Regex regex = new Regex(viewModel.ConnectionFilter, RegexOptions.IgnoreCase | RegexOptions.Multiline);

          viewModel.Connections.ForEach(c => c.IsVisible = regex.IsMatch(c.ToString()));
        }
        catch(ArgumentException) { }
      }
      else viewModel.Connections.ForEach(c => c.IsVisible = true);

      if (viewModel.IsEstablished) viewModel.Connections.Where(c => c.IsVisible).ForEach(c => c.IsVisible = c.State == "Established");

      viewModel.UiPass = watch.ElapsedMilliseconds;

      displayTimer.Start();
    }

    private void onMemoryTimerTick(object sender, EventArgs args) {
      viewModel.TcpConnections = viewModel.Connections.Count(c => c.ConnectionType.StartsWith("TCP"));
      viewModel.UdpConnections = viewModel.Connections.Count(c => c.ConnectionType.StartsWith("UDP"));

      using(Process process = Process.GetCurrentProcess()) {
        viewModel.Memory = process.WorkingSet64 / 1024.0 / 1024.0;
      }
    }

    #endregion Private Methods

  }

}
