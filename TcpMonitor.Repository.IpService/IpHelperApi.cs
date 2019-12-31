using System.Runtime.InteropServices;


namespace TcpMonitor.Repository.IpService {

  public static class IpHelperApi {

    [DllImport("iphlpapi.dll", SetLastError=true)]
    public static extern int GetExtendedTcpTable(byte[] tcpTable, out int size, bool order, uint ipVersion, TcpTableClass tableClass, uint reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern int GetExtendedUdpTable(byte[] udpTable, out int size, bool order, uint ipVersion, UdpTableClass tableClass, uint reserved);

  }

}
