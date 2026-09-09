using Newtonsoft.Json;

using Str.Common.Extensions;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;


namespace TcpMonitor.Repository.Repositories;


public class SettingsRepository : IWindowSettingsRepository {

    #region Private Fields

    private static readonly string WindowSettingsFile;

    #endregion Private Fields

    #region Constructor

    static SettingsRepository() {
        WindowSettingsFile  = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"STR Programming Services\TCP Monitor\WindowSettings.json");
    }

    #endregion Constructor

    #region IWindowSettingsRepository Implementation

    public async Task<DomainWindowSettings> LoadWindowSettingsAsync() {
        DomainWindowSettings? settings = null;

        if (File.Exists(WindowSettingsFile)) {
            settings = JsonConvert.DeserializeObject<DomainWindowSettings>(await File.ReadAllTextAsync(WindowSettingsFile).Fire());
        }

        return settings ?? new DomainWindowSettings {
            WindowW = 1024,
            WindowH = 768,
            WindowX = 100,
            WindowY = 100,
        };
    }

    public async Task SaveWindowSettingsAsync(DomainWindowSettings settings) {
        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

        if (!File.Exists(WindowSettingsFile)) {
            string path = Path.GetDirectoryName(WindowSettingsFile)
                       ?? throw new ArgumentException($"The Windows Settings File path, {WindowSettingsFile}, is not valid.");

            Directory.CreateDirectory(path);
        }

        await File.WriteAllTextAsync(WindowSettingsFile, json).Fire();
    }

    #endregion IWindowSettingsRepository Implementation

}
