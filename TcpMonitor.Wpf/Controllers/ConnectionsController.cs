using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
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
  public class ConnectionsController : IController {

    #region Private Fields

    private readonly Object EntityLock = new Object();

    private readonly List<DomainConnection> connections;

    private readonly DispatcherTimer connectionsTimer;
    private readonly DispatcherTimer     displayTimer;

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

      connectionsTimer = new DispatcherTimer();

      displayTimer = new DispatcherTimer();

      comparer = new ConnectionViewEntityComparer();
    }

    #endregion Constructor

    #region IController Implementation

    public int InitializePriority { get; } = 100;

    public async Task InitializeAsync() {
      connectionsTimer.Tick    += onConnectionsTimerTick;
      connectionsTimer.Interval = TimeSpan.FromMilliseconds(250);

      connectionsTimer.Start();

      displayTimer.Tick    += onDisplayTimerTick;
      displayTimer.Interval = TimeSpan.FromMilliseconds(750);

      displayTimer.Start();

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

      lock(EntityLock) locals = viewModel.Connections.Where(c => c.Key == packet.Key1).ToList();

      locals.ForEach(local => {
        local.PacketsSent = (local.PacketsSent ?? 0) + 1;
        local.BytesSent   = (local.BytesSent   ?? 0) + packet.Bytes;

        local.HasData = true;
      });

      List<ConnectionViewEntity> remotes;

      lock(EntityLock) remotes = viewModel.Connections.Where(c => c.Key == packet.Key2).ToList();

      remotes.ForEach(remote => {
        remote.PacketsReceived = (remote.PacketsReceived ?? 0) + 1;
        remote.BytesReceived   = (remote.BytesReceived   ?? 0) + packet.Bytes;

        remote.HasData = true;
      });
    }

    private async void onConnectionsTimerTick(object sender, EventArgs args) {
      connectionsTimer.Stop();

      List<DomainConnection> incoming = await connectionService.GetConnectionsAsync();

      incoming.Where(c => c.Pid == 0).ToList().ForEach(c => incoming.Remove(c));

      List<DomainConnection> mods = (from row1 in incoming
                                     join row2 in connections on row1.Key equals row2.Key
                                   select row1).ToList();

      mods.ForEach(mod => {
        List<DomainConnection> matches = connections.Where(c => c.Key == mod.Key).ToList();

        matches.ForEach(match => mapper.Map(mod, match));
      });

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

      connectionsTimer.Start();
    }

    private void onDisplayTimerTick(object sender, EventArgs args) {
      viewModel.Connections.ForEach(c => {
        c.IsNew      = false;
        c.HasChanged = false;
        c.HasData    = false;
      });

      List<ConnectionViewEntity> closed = viewModel.Connections.Where(c => c.IsClosed).ToList();

      lock(EntityLock) closed.ForEach(c => viewModel.Connections.Remove(c));

      List<DomainConnection> mods = (from row1 in connections
                                     join row2 in viewModel.Connections on row1.Key equals row2.Key
                                   select row1).ToList();

      mods.ForEach(mod => {
        List<ConnectionViewEntity> matches = viewModel.Connections.Where(c => c.Key == mod.Key).ToList();

        matches.ForEach(match => {
          if (mod.Pid != 0 && match.Pid != 0 && mod.Pid != match.Pid) return;

          if ((mod.Pid != 0 && match.Pid == 0) || mod.ProcessName != match.ProcessName || mod.State != match.State || mod.LocalHostName != match.LocalHostName || mod.RemoteHostName != match.RemoteHostName) {
            mapper.Map(mod, match);

            match.HasChanged = true;
          }
        });
      });

      List<DomainConnection> adds = (from row1 in connections
                                     join row2 in viewModel.Connections on row1.Key equals row2.Key into collGroup
                                     from sub  in collGroup.DefaultIfEmpty()
                                    where sub == null
                                   select row1).ToList();

      List<ConnectionViewEntity> added = mapper.Map<List<ConnectionViewEntity>>(adds.Where(add => !String.IsNullOrEmpty(add.ProcessName)));

      added.ForEach(add => add.IsNew = true);

      lock(EntityLock) viewModel.Connections.OrderedMerge(added.OrderBy(add => add, comparer));

      List<ConnectionViewEntity> dels = (from row1 in viewModel.Connections
                                         join row2 in connections on row1.Key equals row2.Key into collGroup
                                         from sub  in collGroup.DefaultIfEmpty()
                                        where sub == null
                                       select row1).ToList();

      dels.ForEach(del => del.IsClosed = true);

      lock(EntityLock) viewModel.Connections.Sort(comparer);
    }

    #endregion Private Methods

  }

}
