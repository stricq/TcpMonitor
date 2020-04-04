using System.Diagnostics.CodeAnalysis;

using Str.Common.Core;

using Str.MvvmCommon.Core;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels {

  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public sealed class ConnectionsViewModel : ObservableObject {

    #region Private Fields

    private bool isEstablished;

    private bool isFiltered;

    private bool viewPidZero;

    private bool viewDropped;

    private long uiPass;
    private long connectionsPass;

    private double memory;

    private int tcpConnections;
    private int udpConnections;

    private int droppedPackets;

    private string connectionFilter;

    private LockingObservableCollection<ConnectionViewEntity> connections;

    #endregion Private Fields

    #region Properties

    public string ConnectionFilter {
      get => connectionFilter;
      set { SetField(ref connectionFilter, value, () => ConnectionFilter); }
    }

    public bool IsEstablished {
      get => isEstablished;
      set { SetField(ref isEstablished, value, () => IsEstablished); }
    }

    public bool IsFiltered {
      get => isFiltered;
      set { SetField(ref isFiltered, value, () => IsFiltered); }
    }

    public bool ViewPidZero {
      get => viewPidZero;
      set { SetField(ref viewPidZero, value, () => ViewPidZero); }
    }

    public bool ViewDropped {
      get => viewDropped;
      set { SetField(ref viewDropped, value, () => ViewDropped); }
    }

    public LockingObservableCollection<ConnectionViewEntity> Connections {
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

    public long UiPass {
      get => uiPass;
      set { SetField(ref uiPass, value, () => UiPass); }
    }

    public long ConnectionsPass {
      get => uiPass;
      set { SetField(ref connectionsPass, value, () => ConnectionsPass); }
    }

    public double Memory {
      get => memory;
      set { SetField(ref memory, value, () => Memory); }
    }

    #endregion Properties

  }

}
