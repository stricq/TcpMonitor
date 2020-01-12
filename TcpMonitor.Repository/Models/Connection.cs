using System.Diagnostics.CodeAnalysis;
using System.Net;


namespace TcpMonitor.Repository.Models {

  [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by Automapper.")]
  [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Used by Automapper.")]
  public class Connection {

    public int Pid { get; set; }

    public string ProcessName { get; set; }

    public string ConnectionType { get; set; }

    public string State { get; set; }

    public IPEndPoint LocalEndPoint { get; set; }

    public IPEndPoint RemoteEndPoint { get; set; }

  }

}
