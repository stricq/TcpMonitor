using System.Diagnostics.CodeAnalysis;

using Str.Common.Core;

using Str.MvvmCommon.Core;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels;


[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Dependency Injection by Convention.")]
public sealed class ConnectionsViewModel : ObservableObject {

    #region Properties

    public string? ConnectionFilter {
        get;
        set { SetField(ref field, value, () => ConnectionFilter); }
    }

    public bool IsEstablished {
        get;
        set { SetField(ref field, value, () => IsEstablished); }
    }

    public bool IsFiltered {
        get;
        set { SetField(ref field, value, () => IsFiltered); }
    }

    public bool ViewPidZero {
        get;
        set { SetField(ref field, value, () => ViewPidZero); }
    }

    public bool ViewDropped {
        get;
        set { SetField(ref field, value, () => ViewDropped); }
    }

    public LockingObservableCollection<ConnectionViewEntity> Connections {
        get;
        set => SetField(ref field, value, () => Connections);
    } = [];

    public int TcpConnections {
        get;
        set { SetField(ref field, value, () => TcpConnections); }
    }

    public int UdpConnections {
        get;
        set { SetField(ref field, value, () => UdpConnections); }
    }

    public int DroppedPackets {
        get;
        set { SetField(ref field, value, () => DroppedPackets); }
    }

    public long UiPass {
        get;
        set { SetField(ref field, value, () => UiPass); }
    }

    public long ConnectionsPass {
        get;
        set { SetField(ref field, value, () => ConnectionsPass); }
    }

    public int PacketQueue {
        get;
        set { SetField(ref field, value, () => PacketQueue); }
    }

    public double Memory {
        get;
        set { SetField(ref field, value, () => Memory); }
    }

    #endregion Properties

}
