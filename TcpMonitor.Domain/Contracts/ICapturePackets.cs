using System;

using TcpMonitor.Domain.Models;


namespace TcpMonitor.Domain.Contracts {

  public interface ICapturePackets {

    void RegisterPacketCapture(Action<DomainPacket> Callback);

    void UnregisterPacketCapture();

  }

}
