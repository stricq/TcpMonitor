using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

using AutoMapper;

using STR.Common.Extensions;

using STR.MvvmCommon.Contracts;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Wpf.ViewEntities;
using TcpMonitor.Wpf.ViewModels;


namespace TcpMonitor.Wpf.Controllers {

  [Export(typeof(IController))]
  public class ConnectionsController : IController {

    #region Private Fields

    private List<DomainConnection> connections;

    private readonly DispatcherTimer timer;

    private readonly ConnectionsViewModel viewModel;

    private readonly IMapper mapper;

    private readonly IConnectionsService connectionService;

    #endregion Private Fields

    #region Constructor

    [ImportingConstructor]
    public ConnectionsController(ConnectionsViewModel ViewModel, IMapper Mapper, IConnectionsService ConnectionService) {
      viewModel = ViewModel;

      viewModel.Connections = new ObservableCollection<ConnectionViewEntity>();

      mapper = Mapper;

      connectionService = ConnectionService;

      connections = new List<DomainConnection>();

      timer = new DispatcherTimer();
    }

    #endregion Constructor

    #region IController Implementation

    public int InitializePriority { get; } = 100;

    public async Task InitializeAsync() {
      timer.Tick    += onTimerTick;
      timer.Interval = TimeSpan.FromSeconds(1);

      timer.Start();

      await Task.CompletedTask;
    }

    #endregion IController Implementation

    #region Private Methods

    private async void onTimerTick(object sender, EventArgs args) {
      timer.Stop();

      List<DomainConnection> incoming = await connectionService.GetConnectionsAsync();

      List<DomainConnection> mods = (from row1 in incoming
                                     join row2 in connections on row1.Key equals row2.Key
                                   select row1).ToList();

      mods.ForEach(mod => {
        List<DomainConnection> matches = connections.Where(connection => connection.Key == mod.Key).ToList();

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

      viewModel.Connections.Clear();

      viewModel.Connections.AddRange(mapper.Map<List<ConnectionViewEntity>>(connections));

      timer.Start();
    }

    #endregion Private Methods

  }

}
