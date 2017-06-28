using System;
using STR.MvvmCommon;


namespace TcpMonitor.Wpf.ViewEntities {

  public class ConnectionViewEntity : ObservableObject, IComparable<ConnectionViewEntity> {

    #region Private Fields

    private bool hasChanged;
    private bool hasData;
    private bool isClosed;
    private bool isNew;
    private bool isSelected;

    private int pid;

    private string processName;
    private string connectionType;
    private string state;

    private string localAddress;
    private string localHostName;
    private int    localPort;

    private string remoteAddress;
    private string remoteHostName;
    private int    remotePort;

    private long? packetsSent;
    private long?   bytesSent;

    private long? packetsReceived;
    private long?   bytesReceived;

    #endregion Private Fields

    #region Properties

    public string Key { get; set; }

    public bool HasChanged {
      get => hasChanged;
      set { SetField(ref hasChanged, value, () => HasChanged); }
    }

    public bool HasData {
      get => hasData;
      set { SetField(ref hasData, value, () => HasData); }
    }

    public bool IsClosed {
      get => isClosed;
      set { SetField(ref isClosed, value, () => IsClosed); }
    }

    public bool IsNew {
      get => isNew;
      set { SetField(ref isNew, value, () => IsNew); }
    }

    public bool IsSelected {
      get => isSelected;
      set { SetField(ref isSelected, value, () => IsSelected); }
    }

    public int Pid {
      get => pid;
      set { SetField(ref pid, value, () => Pid); }
    }

    public string ProcessName {
      get => processName;
      set { SetField(ref processName, value, () => ProcessName); }
    }

    public string ConnectionType {
      get => connectionType;
      set { SetField(ref connectionType, value, () => ConnectionType); }
    }

    public string State {
      get => state;
      set { SetField(ref state, value, () => State); }
    }

    public string LocalAddress {
      get => localAddress;
      set { SetField(ref localAddress, value, () => LocalAddress); }
    }

    public string LocalHostName {
      get => String.IsNullOrEmpty(localHostName) ? localAddress : localHostName;
      set { SetField(ref localHostName, value, () => LocalHostName); }
    }

    public int LocalPort {
      get => localPort;
      set { SetField(ref localPort, value, () => LocalPort); }
    }

    public string RemoteAddress {
      get => remoteAddress;
      set { SetField(ref remoteAddress, value, () => RemoteAddress); }
    }

    public string RemoteHostName {
      get => String.IsNullOrEmpty(remoteHostName) ? remoteAddress : remoteHostName;
      set { SetField(ref remoteHostName, value, () => RemoteHostName); }
    }

    public int RemotePort {
      get => remotePort;
      set { SetField(ref remotePort, value, () => RemotePort); }
    }

    public long? PacketsSent {
      get => packetsSent;
      set { SetField(ref packetsSent, value, () => PacketsSent); }
    }

    public long? BytesSent {
      get => bytesSent;
      set { SetField(ref bytesSent, value, () => BytesSent); }
    }

    public long? PacketsReceived {
      get => packetsReceived;
      set { SetField(ref packetsReceived, value, () => PacketsReceived); }
    }

    public long? BytesReceived {
      get => bytesReceived;
      set { SetField(ref bytesReceived, value, () => BytesReceived); }
    }

    #endregion Properties

    #region IComparable Implementation

    public int CompareTo(ConnectionViewEntity other) {
      int compared = String.Compare(ProcessName, other.ProcessName, StringComparison.Ordinal);

      if (compared == 0) compared = LocalPort.CompareTo(other.LocalPort);

      if (compared == 0) compared = RemotePort.CompareTo(other.RemotePort);

      return compared;
    }

    #endregion IComparable Implementation

  }

}
