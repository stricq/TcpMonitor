using System.Runtime.InteropServices;


namespace TcpMonitor.Repository.IpService {

  public static class IpHelperApi {

    [DllImport("iphlpapi.dll", SetLastError=true)]
    public static extern int GetExtendedTcpTable(byte[] TcpTable, out int Size, bool Order, uint IPVersion, TcpTableClass TableClass, uint Reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern int GetExtendedUdpTable(byte[] UdpTable, out int Size, bool Order, uint IPVersion, UdpTableClass TableClass, uint Reserved);

  }

}
