using System;
using System.Threading.Tasks;

using TcpMonitor.Domain.Models;


namespace TcpMonitor.Domain.Contracts {

  public interface ICapturePackets {

    Task RegisterPacketCaptureAsync(Action<DomainPacket> packetCallback, Action<DomainDeviceError> deviceCallback);

    void UnregisterPacketCapture();

  }

}
