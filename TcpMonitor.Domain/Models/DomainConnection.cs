using System;
using System.Net;
using System.Threading.Tasks;

using TcpMonitor.Domain.Contracts;


namespace TcpMonitor.Domain.Models {

  public class DomainConnection {

    #region Properties

    public string Key { get; set; }

    public int Pid { get; set; }

    public string ProcessName { get; set; }

    public string ConnectionType { get; set; }

    public string State { get; set; }

    public IPEndPoint LocalEndPoint { get; set; }

    public IPEndPoint RemoteEndPoint { get; set; }

    public string LocalHostName { get; set; }

    public string RemoteHostName { get; set; }

    #endregion Properties

    #region Domain Methods

    public async Task ResolveHostNames(IConnectionsService connectionsService) {
      LocalHostName = await connectionsService.GetHostNameAsync(LocalEndPoint);

      RemoteHostName = await connectionsService.GetHostNameAsync(RemoteEndPoint);
    }

    #endregion Domain Methods

    #region Overrides

    public override String ToString() {
      return Key;
    }

    #endregion Overrides

  }

}
