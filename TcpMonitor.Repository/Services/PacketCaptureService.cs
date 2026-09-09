using System.Collections.Concurrent;
using System.Net;

using PacketDotNet;

using SharpPcap;

using Str.Common.Extensions;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;


namespace TcpMonitor.Repository.Services;


public sealed class PacketCaptureService : ICapturePackets {

    #region Private Fields

    private readonly ConcurrentBag<ICaptureDevice> startedDevices = [];

    private bool initialized;

    private Action<DomainPacket>? callback;

    private CaptureDeviceList? devices;

    #endregion Private Fields

    #region ICapturePackets Implementation

    public async Task RegisterPacketCaptureAsync(Action<DomainPacket> packetCallback, Action<DomainDeviceError> deviceCallback) {
        callback = packetCallback;

        DomainDeviceError deviceMessage = new();

        try {
            devices = CaptureDeviceList.Instance;
        }
        catch { // Most likely reason is WinPCAP is not installed
            deviceMessage.NoDevicesError = true;

            deviceCallback(deviceMessage);

            return;
        }

        await devices.ForEachAsync(device => {
            try {
                deviceMessage.DeviceName = device.Description;

                device.OnPacketArrival += OnDevicePacketArrival;

                device.Open(new DeviceConfiguration { Mode = DeviceModes.None, ReadTimeout = 1000 } /* DeviceMode.Normal, 1000 */);

                device.Filter = "(ip or ip6) and (tcp or udp)";

                device.StartCapture();

                deviceMessage.DeviceLoaded = true;

                startedDevices.Add(device);

                initialized = true;
            }
            catch {
                deviceMessage.DeviceLoaded = false;
            }

            deviceCallback(deviceMessage);

            return Task.CompletedTask;
        }).Fire();
    }

    public void UnregisterPacketCapture() {
        if (!initialized) return;

        foreach(ICaptureDevice device in startedDevices) {
            try {
                device.StopCapture();
            }
            catch {
                // Ignore device errors
            }
        }
    }

    #endregion ICapturePackets Implementation

    #region Private Methods

    private void OnDevicePacketArrival(object sender, PacketCapture args) {
        Packet packet;

        try {
            packet = Packet.ParsePacket(args.GetPacket().LinkLayerType, args.GetPacket().Data);
        }
        catch(Exception ex) when (ex is ArgumentOutOfRangeException or IndexOutOfRangeException) {
            return;
        }

        int length = args.GetPacket().Data.Length;

        DomainPacket domainPacket = new() { Bytes = length };

        try {
            if (packet.Extract<TcpPacket>() is { } tcpPacket) {
                if (tcpPacket.ParentPacket is not IPPacket ipPacket) return;

                domainPacket.SourceEndPoint      = new IPEndPoint(ipPacket.SourceAddress,      tcpPacket.SourcePort);
                domainPacket.DestinationEndPoint = new IPEndPoint(ipPacket.DestinationAddress, tcpPacket.DestinationPort);

                domainPacket.ConnectionType = ipPacket.Version == IPVersion.IPv6 ? "TCPv6" : "TCP";

                domainPacket.Key1 = $"{domainPacket.ConnectionType}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}";
                domainPacket.Key2 = $"{domainPacket.ConnectionType}/{domainPacket.DestinationEndPoint.Address}/{domainPacket.DestinationEndPoint.Port}/{domainPacket.SourceEndPoint.Address}/{domainPacket.SourceEndPoint.Port}";
            }
            else if (packet.Extract<UdpPacket>() is { } udpPacket) {
                if (udpPacket.ParentPacket is not IPPacket ipPacket) return;

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

        callback?.Invoke(domainPacket);
    }

    #endregion Private Methods

}
