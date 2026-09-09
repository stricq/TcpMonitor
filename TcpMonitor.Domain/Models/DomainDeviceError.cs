using System.Diagnostics.CodeAnalysis;

namespace TcpMonitor.Domain.Models;


[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class DomainDeviceError {

    public bool NoDevicesError { get; set; }

    public string? DeviceName { get; set; }

    public bool DeviceLoaded { get; set; }

}
