using System.Net;

using TcpMonitor.Domain.Models;


namespace TcpMonitor.Domain.Contracts;


public interface IConnectionsService {

    Task<List<DomainConnection>> GetConnectionsAsync();

    Task<string> GetHostNameAsync(IPEndPoint hostAddress);

    bool IsLocalAddress(IPEndPoint? hostAddress);

    Task<string> GetProcessNameAsync(int pid);

}
