using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using TcpMonitor.Domain.Models;


namespace TcpMonitor.Domain.Contracts {

  public interface IConnectionsService {

    Task<List<DomainConnection>> GetConnectionsAsync();

    Task<string> GetHostNameAsync(IPEndPoint hostAddress);

  }

}
