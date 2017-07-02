using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

using STR.MvvmCommon;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels {

  [Export]
  [ViewModel(nameof(ConnectionsViewModel))]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public class ConnectionsViewModel : ObservableObject {

    #region Private Fields

    public bool isFiltered;

    private double memory;

    private int tcpConnections;
    private int udpConnections;

    private int droppedPackets;

    private string connectionFilter;

    private ObservableCollection<ConnectionViewEntity> connections;

    #endregion Private Fields

    #region Properties

    public string ConnectionFilter {
      get => connectionFilter;
      set { SetField(ref connectionFilter, value, () => ConnectionFilter); }
    }

    public bool IsFiltered {
      get => isFiltered;
      set { SetField(ref isFiltered, value, () => IsFiltered); }
    }

    public ObservableCollection<ConnectionViewEntity> Connections {
      get => connections;
      set { SetField(ref connections, value, () => Connections); }
    }

    public int TcpConnections {
      get => tcpConnections;
      set { SetField(ref tcpConnections, value, () => TcpConnections); }
    }

    public int UdpConnections {
      get => udpConnections;
      set { SetField(ref udpConnections, value, () => UdpConnections); }
    }

    public int DroppedPackets {
      get => droppedPackets;
      set { SetField(ref droppedPackets, value, () => DroppedPackets); }
    }

    public double Memory {
      get => memory;
      set { SetField(ref memory, value, () => Memory); }
    }

    #endregion Properties

  }

}
