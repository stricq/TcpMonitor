using System;
using System.ComponentModel.Composition;
using System.Net;

using PacketDotNet;

using SharpPcap;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;


namespace TcpMonitor.Repository.Services {

  [Export(typeof(ICapturePackets))]
  public class PacketCaptureService : ICapturePackets {

    #region Private Fields

    private Action<DomainPacket> callback;

    #endregion Private Fields

    #region ICapturePackets Implementation

    public void RegisterPacketCapture(Action<DomainPacket> Callback) {
      callback = Callback;

      CaptureDeviceList devices = CaptureDeviceList.Instance;

      foreach(ICaptureDevice device in devices) {
        //
        // ReSharper disable once EmptyGeneralCatchClause
        //
        try {
          device.OnPacketArrival += onDevicePacketArrival;

          device.Open(DeviceMode.Normal, 1000);

          device.Filter = "(ip or ip6) and (tcp or udp)";

          device.StartCapture();
        }
        catch { } // Ignore errors
      }
    }

    public void UnregisterPacketCapture() {
      CaptureDeviceList devices = CaptureDeviceList.Instance;

      foreach(ICaptureDevice device in devices) {
        //
        // ReSharper disable once EmptyGeneralCatchClause
        //
        try {
          device.StopCapture();
        }
        catch { } // Ignore errors
      }
    }

    #endregion ICapturePackets Implementation

    #region Private Methods

    private void onDevicePacketArrival(object sender, CaptureEventArgs args) {
      Packet packet;

      try {
        packet = Packet.ParsePacket(args.Packet.LinkLayerType, args.Packet.Data);
      }
      catch(IndexOutOfRangeException) {
        return;
      }

      int length = args.Packet.Data.Length;

      TcpPacket tcpPacket = packet.Extract(typeof(TcpPacket)) as TcpPacket;
      UdpPacket udpPacket = packet.Extract(typeof(UdpPacket)) as UdpPacket;

      DomainPacket domainPacket = new DomainPacket { Bytes = length };

      if (tcpPacket != null) {
        IpPacket ipPacket = tcpPacket.ParentPacket as IpPacket;

        if (ipPacket == null) return;

        domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      tcpPacket.SourcePort);
        domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, tcpPacket.DestinationPort);

        domainPacket.ConnectionType = ipPacket.Version == IpVersion.IPv6 ? "TCPv6" : "TCP";

        domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
        domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
      }
      else if (udpPacket != null) {
        IpPacket ipPacket = udpPacket.ParentPacket as IpPacket;

        if (ipPacket == null) return;

        domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      udpPacket.SourcePort);
        domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, udpPacket.DestinationPort);

        domainPacket.ConnectionType = ipPacket.Version == IpVersion.IPv6 ? "UDPv6" : "UDP";

        domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
        domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
      }

      callback(domainPacket);
    }

    #endregion Private Methods

  }

}
