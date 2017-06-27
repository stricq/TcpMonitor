using System.Net;


namespace TcpMonitor.Repository.Models.IpService {

  public class Connection {

    public int Pid { get; set; }

    public string ProcessName { get; set; }

    public string ConnectionType { get; set; }

    public string State { get; set; }

    public IPEndPoint LocalEndPoint { get; set; }

    public IPEndPoint RemoteEndPoint { get; set; }

  }

}
