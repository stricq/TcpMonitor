using System.Diagnostics.CodeAnalysis;


namespace TcpMonitor.Domain.Models {

  [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
  public class DomainWindowSettings {

    public double WindowW { get; set; }

    public double WindowH { get; set; }

    public double WindowX { get; set; }

    public double WindowY { get; set; }

  }

}
