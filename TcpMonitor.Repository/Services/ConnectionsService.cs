﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

using AutoMapper;

using STR.Common.Extensions;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Repository.IpService;

using TcpMonitor.Repository.Models.IpService;


namespace TcpMonitor.Repository.Services {

  [Export(typeof(IConnectionsService))]
  public class ConnectionsService : IConnectionsService {

    #region Private Fields

    private readonly Func<List<Connection>>[] tables = { GetExtendedTcpTable4, GetExtendedTcpTable6, GetExtendedUdpTable4, GetExtendedUdpTable6 };

    private readonly IMapper mapper;

    #endregion Private Fields

    #region Constructor

    [ImportingConstructor]
    public ConnectionsService(IMapper Mapper) {
      mapper = Mapper;
    }

    #endregion Constructor

    #region IConnectionsService Implementation

    public async Task<List<DomainConnection>> GetConnectionsAsync() {
      ConcurrentBag<Connection> connections = new ConcurrentBag<Connection>();

      await tables.ForEachAsync(table => Task.Run(table).ContinueWith(task => {
        if (task.IsCompleted && !task.IsFaulted) task.Result.ForEach(item => connections.Add(item));
      }));

      return mapper.Map<List<DomainConnection>>(connections);
    }

    public async Task<string> GetHostNameAsync(IPEndPoint hostAddress) {
      string addr = hostAddress.Address.ToString();

      if (addr == "0.0.0.0" || addr == "::") return "*";

      return await Task.Run(() => {
        try {
          return Dns.GetHostEntry(hostAddress.Address).HostName;
        }
        catch {
          return addr;
        }
      });
    }

    public async Task<string> GetProcessNameAsync(int pid) {
      if (pid == 0) return "Unknown";

      return await Task.Run(() => {
        try {
          using(Process p = Process.GetProcessById(pid)) {
            return p.ProcessName;
          }
        }
        catch {
          return "Unknown";
        }
      });
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
        Connection tcp = new Connection { ConnectionType = "TCP" };

        tcp.State = ConvertState(BitConverter.ToInt32(tcpTable, index)); index += 4;

        uint localAddr = BitConverter.ToUInt32(tcpTable, index); index += 4;
        uint localPort = BitConverter.ToUInt32(tcpTable, index); index += 4;

        tcp.LocalEndPoint = new IPEndPoint((long)localAddr, (int)ConvertPort(localPort));

        uint remoteAddr = BitConverter.ToUInt32(tcpTable, index); index += 4;
        uint remotePort = BitConverter.ToUInt32(tcpTable, index); index += 4;

        tcp.RemoteEndPoint = new IPEndPoint((long)remoteAddr, (int)ConvertPort(remotePort));

        tcp.Pid = BitConverter.ToInt32(tcpTable, index); index += 4;

//      tcp.ProcessName = GetProcessNameByPid(tcp.Pid);

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

//      tcp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, localScope), (int)ConvertPort(localPort));
        tcp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, 0), (int)ConvertPort(localPort));

        Array.Copy(tcpTable, index, remoteAddr, 0, 16); index += 16;

        uint remoteScope = BitConverter.ToUInt32(tcpTable, index); index += 4;
        uint remotePort  = BitConverter.ToUInt32(tcpTable, index); index += 4;

//      tcp.RemoteEndPoint = new IPEndPoint(new IPAddress(remoteAddr, remoteScope), (int)ConvertPort(remotePort));
        tcp.RemoteEndPoint = new IPEndPoint(new IPAddress(remoteAddr, 0), (int)ConvertPort(remotePort));

        tcp.State = ConvertState(BitConverter.ToInt32(tcpTable, index)); index += 4;

        tcp.Pid = BitConverter.ToInt32(tcpTable, index); index += 4;

//      tcp.ProcessName = GetProcessNameByPid(tcp.Pid);

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

//      udp.ProcessName = GetProcessNameByPid(udp.Pid);

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

//      udp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, localScope), (int)ConvertPort(localPort));
        udp.LocalEndPoint = new IPEndPoint(new IPAddress(localAddr, 0), (int)ConvertPort(localPort));

        udp.RemoteEndPoint = new IPEndPoint(new IPAddress(remoteAddr, 0), 0);

        udp.State = "Listen";

        udp.Pid = BitConverter.ToInt32(udpTable, index); index += 4;

//      udp.ProcessName = GetProcessNameByPid(udp.Pid);

        table.Add(udp);
      }

      return table;
    }

    private static string ConvertState(int state) {
      switch(state) {
        case State.MIB_TCP_STATE_CLOSED     : return "Closed";
        case State.MIB_TCP_STATE_LISTEN     : return "Listen";
        case State.MIB_TCP_STATE_SYN_SENT   : return "SYN Sent";
        case State.MIB_TCP_STATE_SYN_RCVD   : return "SYN Recieved";
        case State.MIB_TCP_STATE_ESTAB      : return "Established";
        case State.MIB_TCP_STATE_FIN_WAIT1  : return "FIN Wait 1";
        case State.MIB_TCP_STATE_FIN_WAIT2  : return "FIN Wait 2";
        case State.MIB_TCP_STATE_CLOSE_WAIT : return "Close Wait";
        case State.MIB_TCP_STATE_CLOSING    : return "Closing";
        case State.MIB_TCP_STATE_LAST_ACK   : return "Last ACK";
        case State.MIB_TCP_STATE_TIME_WAIT  : return "Time Wait";
        case State.MIB_TCP_STATE_DELETE_TCB : return "Delete TCB";
      }

      return "Unknown";
    }

    private static uint ConvertPort(uint port) {
      byte[] bytes = BitConverter.GetBytes(port);

      uint lo = bytes[0];
      uint hi = bytes[1];

      return (lo << 8) | hi;
    }

    #endregion Private Methods

  }

}
