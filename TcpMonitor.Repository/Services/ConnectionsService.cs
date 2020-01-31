using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using AutoMapper;

using Str.Common.Extensions;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Repository.IpService;
using TcpMonitor.Repository.Models;


namespace TcpMonitor.Repository.Services {

  [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Assignment is requireed by the called dll.")]
  public class ConnectionsService : IConnectionsService {

    #region Private Fields

    private readonly ConcurrentDictionary<IPAddress, string> dnsCache;

    private readonly IPAddress[] localAddresses;

    private readonly Func<List<Connection>>[] tables = { GetExtendedTcpTable4, GetExtendedTcpTable6, GetExtendedUdpTable4, GetExtendedUdpTable6 };

    private readonly IMapper mapper;

    #endregion Private Fields

    #region Constructor

    public ConnectionsService(IMapper mapper) {
      this.mapper = mapper;

      dnsCache = new ConcurrentDictionary<IPAddress, string>();

      localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
    }

    #endregion Constructor

    #region IConnectionsService Implementation

    public async Task<List<DomainConnection>> GetConnectionsAsync() {
      ConcurrentBag<Connection> connections = new ConcurrentBag<Connection>();

      await tables.ForEachAsync(table => Task.Run(table).ContinueWith(task => {
        if (task.IsCompletedSuccessfully) task.FireAndWait().ForEach(item => connections.Add(item));
      })).Fire();

      return mapper.Map<List<DomainConnection>>(connections);
    }

    public async Task<string> GetHostNameAsync(IPEndPoint hostAddress) {
      if (dnsCache.ContainsKey(hostAddress.Address)) return dnsCache[hostAddress.Address];

      if (localAddresses.Contains(hostAddress.Address) || hostAddress.Address.IsIPv6SiteLocal) return Dns.GetHostName();

      string addr = hostAddress.Address.ToString();

      if (addr == "0.0.0.0" || addr == "::") return "*";

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

    public bool IsLocalAddress(IPEndPoint hostAddress) {
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

    private static List<Connection> GetExtendedTcpTable4() {
      IpHelperApi.GetExtendedTcpTable(null, out int size, true, AfInet.AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

      byte[] tcpTable = new byte[size];

      IpHelperApi.GetExtendedTcpTable(tcpTable, out size, true, AfInet.AF_INET, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

      int index = 0;

      int entries = BitConverter.ToInt32(tcpTable, index); index += 4;

      List<Connection> table = new List<Connection>(entries);

      for(int i = 0; i < entries; ++i) {
        Connection tcp = new Connection {
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

    private static List<Connection> GetExtendedTcpTable6() {
      IpHelperApi.GetExtendedTcpTable(null, out int size, true, AfInet.AF_INET6, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

      byte[] tcpTable = new byte[size];

      IpHelperApi.GetExtendedTcpTable(tcpTable, out size, true, AfInet.AF_INET6, TcpTableClass.TCP_TABLE_OWNER_PID_ALL, 0);

      int index = 0;

      int entries = BitConverter.ToInt32(tcpTable, index); index += 4;

      List<Connection> table = new List<Connection>(entries);

      byte[]  localAddr = new byte[16];
      byte[] remoteAddr = new byte[16];

      for(int i = 0; i < entries; ++i) {
        Connection tcp = new Connection { ConnectionType = "TCPv6" };

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

    private static List<Connection> GetExtendedUdpTable4() {
      IpHelperApi.GetExtendedUdpTable(null, out int size, true, AfInet.AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

      byte[] udpTable = new byte[size];

      IpHelperApi.GetExtendedUdpTable(udpTable, out size, true, AfInet.AF_INET, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

      int index = 0;

      int entries = BitConverter.ToInt32(udpTable, index); index += 4;

      List<Connection> table = new List<Connection>(entries);

      for(int i = 0; i < entries; ++i) {
        Connection udp = new Connection { ConnectionType = "UDP", State = "Listen" };

        uint localAddr = BitConverter.ToUInt32(udpTable, index); index += 4;
        uint localPort = BitConverter.ToUInt32(udpTable, index); index += 4;

        udp.LocalEndPoint = new IPEndPoint(localAddr, (int)ConvertPort(localPort));

        udp.RemoteEndPoint = new IPEndPoint(0, 0);

        udp.Pid = BitConverter.ToInt32(udpTable, index); index += 4;

        table.Add(udp);
      }

      return table;
    }

    private static List<Connection> GetExtendedUdpTable6() {
      IpHelperApi.GetExtendedUdpTable(null, out int size, true, AfInet.AF_INET6, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

      byte[] udpTable = new byte[size];

      IpHelperApi.GetExtendedUdpTable(udpTable, out size, true, AfInet.AF_INET6, UdpTableClass.UDP_TABLE_OWNER_PID, 0);

      int index = 0;

      int entries = BitConverter.ToInt32(udpTable, index); index += 4;

      List<Connection> table = new List<Connection>(entries);

      byte[]  localAddr = new byte[16];
      byte[] remoteAddr = new byte[16];

      for(int i = 0; i < entries; ++i) {
        Connection udp = new Connection { ConnectionType = "UDPv6" };

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

    private static string ConvertState(int state) {
      return state switch {
        State.MIB_TCP_STATE_CLOSED     => "Closed",
        State.MIB_TCP_STATE_LISTEN     => "Listen",
        State.MIB_TCP_STATE_SYN_SENT   => "SYN Sent",
        State.MIB_TCP_STATE_SYN_RCVD   => "SYN Recieved",
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ConvertPort(uint port) {
      return ((port & 0xff) << 8) | (port >> 8);
    }

    #endregion Private Methods

  }

}
