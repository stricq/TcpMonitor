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

    private Action<DomainPacket> callbackAction;

    private CaptureDeviceList devices;

    #endregion Private Fields

    #region ICapturePackets Implementation

    [SuppressMessage("ReSharper", "EmptyGeneralCatchClause")]
    public void RegisterPacketCapture(Action<DomainPacket> callback) {
      callbackAction = callback;

      try {
        devices = CaptureDeviceList.Instance;
      }
      catch {
        return; // Most likely reason is WinPCAP is not installed
      }

      foreach(ICaptureDevice device in devices) {
        try {
          device.OnPacketArrival += OnDevicePacketArrival;

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

    private void OnDevicePacketArrival(object sender, CaptureEventArgs args) {
      Packet packet;

      try {
        packet = Packet.ParsePacket(args.Packet.LinkLayerType, args.Packet.Data);
      }
      catch(Exception ex) when (ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException) {
        return;
      }

      int length = args.Packet.Data.Length;

      DomainPacket domainPacket = new DomainPacket { Bytes = length };

      try {
        if (packet.Extract<TcpPacket>() is { } tcpPacket) {
          if (!(tcpPacket.ParentPacket is IPPacket ipPacket)) return;

          domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      tcpPacket.SourcePort);
          domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, tcpPacket.DestinationPort);

          domainPacket.ConnectionType = ipPacket.Version == IPVersion.IPv6 ? "TCPv6" : "TCP";

          domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
          domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
        }
        else if (packet.Extract<UdpPacket>() is { } udpPacket) {
          if (!(udpPacket.ParentPacket is IPPacket ipPacket)) return;

          domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      udpPacket.SourcePort);
          domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, udpPacket.DestinationPort);

          domainPacket.ConnectionType = ipPacket.Version == IPVersion.IPv6 ? "UDPv6" : "UDP";

          domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
          domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
        }
      }
      catch(Exception ex) when (ex is ArgumentOutOfRangeException) {
        //
        // PacketDotNet can't parse some TCPv6 packets...
        //
        return;
      }

      callbackAction(domainPacket);
    }

    #endregion Private Methods

  }

}
