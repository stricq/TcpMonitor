using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net;

using PacketDotNet;

using SharpPcap;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;


namespace TcpMonitor.Repository.Services {

  [Export(typeof(ICapturePackets))]
  public sealed class PacketCaptureService : ICapturePackets {

    #region Private Fields

    private bool initialized;

    private Action<DomainPacket> callback;

    private CaptureDeviceList devices;

    #endregion Private Fields

    #region ICapturePackets Implementation

    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    public void RegisterPacketCapture(Action<DomainPacket> Callback) {
      callback = Callback;

      try {
        devices = CaptureDeviceList.Instance;
      }
      catch {
        return; // Most likely reason is WinPCAP is not installed
      }

      foreach(ICaptureDevice device in devices) {
        try {
          device.OnPacketArrival += onDevicePacketArrival;

          device.Open(DeviceMode.Normal, 1000);

          device.Filter = "(ip or ip6) and (tcp or udp)";

          device.StartCapture();

          initialized = true;
        }
        catch { } // Ignore device errors
      }
    }

    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    public void UnregisterPacketCapture() {
      if (!initialized) return;

      foreach(ICaptureDevice device in devices) {
        try {
          device.StopCapture();
        }
        catch { } // Ignore device errors
      }
    }

    #endregion ICapturePackets Implementation

    #region Private Methods

    private void onDevicePacketArrival(object sender, CaptureEventArgs args) {
      Packet packet;

      try {
        packet = Packet.ParsePacket(args.Packet.LinkLayerType, args.Packet.Data);
      }
      catch(ArgumentOutOfRangeException) {
        return;
      }
      catch(IndexOutOfRangeException) {
        return;
      }

      int length = args.Packet.Data.Length;

      DomainPacket domainPacket = new DomainPacket { Bytes = length };

      if (packet.Extract(typeof(TcpPacket)) is TcpPacket tcpPacket) {
        if (!(tcpPacket.ParentPacket is IPPacket ipPacket)) return;

        domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      tcpPacket.SourcePort);
        domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, tcpPacket.DestinationPort);

        domainPacket.ConnectionType = ipPacket.Version == IPVersion.IPv6 ? "TCPv6" : "TCP";

        domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
        domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
      }
      else if (packet.Extract(typeof(UdpPacket)) is UdpPacket udpPacket) {
        if (!(udpPacket.ParentPacket is IPPacket ipPacket)) return;

        domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      udpPacket.SourcePort);
        domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, udpPacket.DestinationPort);

        domainPacket.ConnectionType = ipPacket.Version == IPVersion.IPv6 ? "UDPv6" : "UDP";

        domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
        domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
      }

      callback(domainPacket);
    }

    #endregion Private Methods

  }

}
