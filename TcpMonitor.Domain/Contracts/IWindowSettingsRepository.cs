using System.Threading.Tasks;

using TcpMonitor.Domain.Models;


namespace TcpMonitor.Domain.Contracts {

  public interface IWindowSettingsRepository {

    Task<DomainWindowSettings> LoadWindowSettingsAsync();

    Task SaveWindowSettingsAsync(DomainWindowSettings settings);

  }

}
