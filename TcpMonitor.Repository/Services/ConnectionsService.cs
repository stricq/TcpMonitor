using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;

using AutoMapper;

using Str.Common.Extensions;

using TcpMonitor.Domain.Constants;
using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Repository.IpService.Constants;
using TcpMonitor.Repository.Models;


namespace TcpMonitor.Repository.Services;


public class ConnectionsService : IConnectionsService {

    #region Private Fields

    private readonly ConcurrentDictionary<IPAddress, string> dnsCache;

    private readonly IPAddress[] localAddresses;

    private readonly IIpHelperApi ipHelper;

    private readonly Func<List<Connection>>[] tables;

    private readonly IMapper mapper;

    #endregion Private Fields

    #region Constructor

    public ConnectionsService(IMapper mapper, IIpHelperApi ipHelper) {
        this.mapper = mapper;

        this.ipHelper = ipHelper;

        dnsCache = new ConcurrentDictionary<IPAddress, string>();

        localAddresses = Dns.GetHostAddresses(Dns.GetHostName());

        tables = [GetExtendedTcpTable4, GetExtendedTcpTable6, GetExtendedUdpTable4, GetExtendedUdpTable6];
    }

    #endregion Constructor

    #region IConnectionsService Implementation

    public async Task<List<DomainConnection>> GetConnectionsAsync() {
        ConcurrentBag<Connection> connections = [];

        await tables.ForEachAsync(table => Task.Run(table).ContinueWith(task => {
            if (task.IsCompletedSuccessfully) task.FireAndWait().ForEach(connections.Add);
        })).Fire();

        return mapper.Map<List<DomainConnection>>(connections);
    }

    public async Task<string> GetHostNameAsync(IPEndPoint hostAddress) {
        if (dnsCache.TryGetValue(hostAddress.Address, out string? address)) return address;

        if (localAddresses.Contains(hostAddress.Address) || hostAddress.Address.IsIPv6SiteLocal) return Dns.GetHostName();

        string addr = hostAddress.Address.ToString();

        if (addr is "0.0.0.0" or "::") return "*";

        return await Task.Run(() => {
            try {
                dnsCache[hostAddress.Address] = Dns.GetHostEntry(hostAddress.Address).HostName;

                return dnsCache[hostAddress.Address];
            }
            catch {
                dnsCache[hostAddress.Address] = addr;

                return addr;
            }
        }).Fire();
    }

    public bool IsLocalAddress(IPEndPoint? hostAddress) {
        if (hostAddress == null) return false;

        return localAddresses.Contains(hostAddress.Address) || hostAddress.Address.IsIPv6SiteLocal;
    }

    public async Task<string> GetProcessNameAsync(int pid) {
        if (pid == 0) return "Unknown";

        return await Task.Run(() => {
            try {
                using Process p = Process.GetProcessById(pid);

                return p.ProcessName;
            }
            catch {
                return "Unknown";
            }
        }).Fire();
    }

    #endregion IConnectionsService Implementation

    #region Private Methods

    private List<Connection> GetExtendedTcpTable4() {
        ipHelper.GetExtendedTcpTableHelper(null, out int size, true, AfInet.AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        byte[] tcpTable = new byte[size];

        ipHelper.GetExtendedTcpTableHelper(tcpTable, out size, true, AfInet.AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        int index = 0;

        int entries = BitConverter.ToInt32(tcpTable, index); index = 4;

        List<Connection> table = new(entries);

        for(int i = 0; i < entries; ++i) {
            Connection tcp = new() {
                ConnectionType = "TCP",
                State          = ConvertState(BitConverter.ToInt32(tcpTable, index))
            };

            index += 4;

            uint localAddr = BitConverter.ToUInt32(tcpTable, index); index += 4;
            uint localPort = BitConverter.ToUInt32(tcpTable, index); index += 4;

            tcp.LocalEndPoint = new IPEndPoint(localAddr, (int)ConvertPort(localPort));

            uint remoteAddr = BitConverter.ToUInt32(tcpTable, index); index += 4;
            uint remotePort = BitConverter.ToUInt32(tcpTable, index); index += 4;

            tcp.RemoteEndPoint = new IPEndPoint(remoteAddr, (int)ConvertPort(remotePort));

            tcp.Pid = BitConverter.ToInt32(tcpTable, index); index += 4;

            table.Add(tcp);
        }

        return table;
    }

    private List<Connection> GetExtendedTcpTable6() {
        ipHelper.GetExtendedTcpTableHelper(null, out int size, true, AfInet.AF_INET6, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        byte[] tcpTable = new byte[size];

        ipHelper.GetExtendedTcpTableHelper(tcpTable, out size, true, AfInet.AF_INET6, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

        int index = 0;

        int entries = BitConverter.ToInt32(tcpTable, index); index = 4;

        List<Connection> table = new(entries);

        byte[]  localAddr = new byte[16];
        byte[] remoteAddr = new byte[16];

        for(int i = 0; i < entries; ++i) {
            Connection tcp = new() { ConnectionType = "TCPv6" };

            Array.Copy(tcpTable, index, localAddr, 0, 16); index += 16;

            uint localScope = BitConverter.ToUInt32(tcpTable, index); index += 4;
            uint localPort  = BitConverter.ToUInt32(tcpTable, index); index += 4;

            tcp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, localScope), (int)ConvertPort(localPort));

            Array.Copy(tcpTable, index, remoteAddr, 0, 16); index += 16;

            uint remoteScope = BitConverter.ToUInt32(tcpTable, index); index += 4;
            uint remotePort  = BitConverter.ToUInt32(tcpTable, index); index += 4;

            tcp.RemoteEndPoint = new IPEndPoint(new IPAddress(remoteAddr, remoteScope), (int)ConvertPort(remotePort));

            tcp.State = ConvertState(BitConverter.ToInt32(tcpTable, index)); index += 4;

            tcp.Pid = BitConverter.ToInt32(tcpTable, index); index += 4;

            table.Add(tcp);
        }

        return table;
    }

    private List<Connection> GetExtendedUdpTable4() {
        ipHelper.GetExtendedUdpTableHelper(null, out int size, true, AfInet.AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

        byte[] udpTable = new byte[size];

        ipHelper.GetExtendedUdpTableHelper(udpTable, out size, true, AfInet.AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

        int index = 0;

        int entries = BitConverter.ToInt32(udpTable, index); index = 4;

        List<Connection> table = new(entries);

        for(int i = 0; i < entries; ++i) {
            Connection udp = new() { ConnectionType = "UDP", State = "Listen" };

            uint localAddr = BitConverter.ToUInt32(udpTable, index); index += 4;
            uint localPort = BitConverter.ToUInt32(udpTable, index); index += 4;

            udp.LocalEndPoint = new IPEndPoint(localAddr, (int)ConvertPort(localPort));

            udp.RemoteEndPoint = new IPEndPoint(0, 0);

            udp.Pid = BitConverter.ToInt32(udpTable, index); index += 4;

            table.Add(udp);
        }

        return table;
    }

    private List<Connection> GetExtendedUdpTable6() {
        ipHelper.GetExtendedUdpTableHelper(null, out int size, true, AfInet.AF_INET6, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

        byte[] udpTable = new byte[size];

        ipHelper.GetExtendedUdpTableHelper(udpTable, out size, true, AfInet.AF_INET6, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

        int index = 0;

        int entries = BitConverter.ToInt32(udpTable, index); index = 4;

        List<Connection> table = new(entries);

        byte[]  localAddr = new byte[16];
        byte[] remoteAddr = new byte[16];

        for(int i = 0; i < entries; ++i) {
            Connection udp = new() { ConnectionType = "UDPv6" };

            Array.Copy(udpTable, index, localAddr, 0, 16); index += 16;

            uint localScope = BitConverter.ToUInt32(udpTable, index); index += 4;
            uint localPort  = BitConverter.ToUInt32(udpTable, index); index += 4;

            udp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, localScope), (int)ConvertPort(localPort));

            udp.RemoteEndPoint = new IPEndPoint(new IPAddress(remoteAddr, 0), 0);

            udp.State = "Listen";

            udp.Pid = BitConverter.ToInt32(udpTable, index); index += 4;

            table.Add(udp);
        }

        return table;
    }

    private static string ConvertState(int state) => state switch {
                                                         State.MIB_TCP_STATE_CLOSED     => "Closed",
                                                         State.MIB_TCP_STATE_LISTEN     => "Listen",
                                                         State.MIB_TCP_STATE_SYN_SENT   => "SYN Sent",
                                                         State.MIB_TCP_STATE_SYN_RCVD   => "SYN Received",
                                                         State.MIB_TCP_STATE_ESTAB      => "Established",
                                                         State.MIB_TCP_STATE_FIN_WAIT1  => "FIN Wait 1",
                                                         State.MIB_TCP_STATE_FIN_WAIT2  => "FIN Wait 2",
                                                         State.MIB_TCP_STATE_CLOSE_WAIT => "Close Wait",
                                                         State.MIB_TCP_STATE_CLOSING    => "Closing",
                                                         State.MIB_TCP_STATE_LAST_ACK   => "Last ACK",
                                                         State.MIB_TCP_STATE_TIME_WAIT  => "Time Wait",
                                                         State.MIB_TCP_STATE_DELETE_TCB => "Delete TCB",
                                                         _                              => "Unknown"
                                                     };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ConvertPort(uint port) => ((port & 0xff) << 8) | (port >> 8);

    #endregion Private Methods

}
