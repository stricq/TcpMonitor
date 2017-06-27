using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using STR.MvvmCommon;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels {

  [Export]
  [ViewModel(nameof(ConnectionsViewModel))]
  public class ConnectionsViewModel : ObservableObject {

    #region Private Fields

    private ObservableCollection<ConnectionViewEntity> connections;

    #endregion Private Fields

    #region Properties

    public ObservableCollection<ConnectionViewEntity> Connections {
      get => connections;
      set { SetField(ref connections, value, () => Connections); }
    }

    #endregion Properties

  }

}
