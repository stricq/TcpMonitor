using System.Net;


namespace TcpMonitor.Domain.Models {

  public class DomainPacket {

    #region Properties

    public string Key1 { get; set; }

    public string Key2 { get; set; }

    public string ConnectionType { get; set; }

    public IPEndPoint SourceEndPoint { get; set; }

    public IPEndPoint DestinationEndPoint { get; set; }

    public long Bytes { get; set; }

    #endregion Properties

  }

}
