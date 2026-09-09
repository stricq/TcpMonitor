using System.Diagnostics.CodeAnalysis;

namespace TcpMonitor.Domain.Constants;


[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum UdpTableClass {

    UDP_TABLE_BASIC,
    UDP_TABLE_OWNER_PID,
    UDP_TABLE_OWNER_MODULE

}
