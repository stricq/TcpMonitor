using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

using Str.Common.Extensions;

using TcpMonitor.Domain.Contracts;


namespace TcpMonitor.Domain.Models {

  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global",  Justification = "Instantiated by Automapper.")]
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

    public async Task ResolveHostNamesAsync(IConnectionsService connectionsService) {
      ProcessName = await connectionsService.GetProcessNameAsync(Pid).Fire();

      LocalHostName = await connectionsService.GetHostNameAsync(LocalEndPoint).Fire();

      RemoteHostName = await connectionsService.GetHostNameAsync(RemoteEndPoint).Fire();
    }

    #endregion Domain Methods

    #region Overrides

    public override String ToString() {
      return Key;
    }

    #endregion Overrides

  }

}
