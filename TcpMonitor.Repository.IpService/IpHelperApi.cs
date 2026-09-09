using System.Runtime.InteropServices;

using TcpMonitor.Domain.Constants;
using TcpMonitor.Domain.Contracts;


namespace TcpMonitor.Repository.IpService;


public sealed class IpHelperApi : IIpHelperApi {

    #region IIHelperApi

    public int GetExtendedTcpTableHelper(byte[]? tcpTable, out int size, bool order, uint ipVersion, TcpTableClass tableClass, uint reserved) => GetExtendedTcpTable(tcpTable, out size, order, ipVersion, tableClass, reserved);

    public int GetExtendedUdpTableHelper(byte[]? udpTable, out int size, bool order, uint ipVersion, UdpTableClass tableClass, uint reserved) => GetExtendedUdpTable(udpTable, out size, order, ipVersion, tableClass, reserved);

    #endregion IIHelperApi

    #region Private Methods

    #pragma warning disable SYSLIB1054 // Use LibraryImport - It doesn't work

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetExtendedTcpTable(byte[]? tcpTable, out int size, bool order, uint ipVersion, TcpTableClass tableClass, uint reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetExtendedUdpTable(byte[]? udpTable, out int size, bool order, uint ipVersion, UdpTableClass tableClass, uint reserved);

    #pragma warning restore SYSLIB1054 // Use LibraryImport - It doesn't work

    #endregion Private Methods

}
