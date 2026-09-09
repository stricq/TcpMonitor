using TcpMonitor.Domain.Constants;


namespace TcpMonitor.Domain.Contracts;


public interface IIpHelperApi {

    int GetExtendedTcpTableHelper(byte[]? tcpTable, out int size, bool order, uint ipVersion, TcpTableClass tableClass, uint reserved);

    int GetExtendedUdpTableHelper(byte[]? udpTable, out int size, bool order, uint ipVersion, UdpTableClass tableClass, uint reserved);

}
