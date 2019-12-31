using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;


namespace TcpMonitor.Repository.Repositories {

  [Export(typeof(IWindowSettingsRepository))]
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
      DomainWindowSettings settings;

      if (await Task.Run(() => File.Exists(WindowSettingsFile))) {
        settings = await Task.Run(() => JsonConvert.DeserializeObject<DomainWindowSettings>(File.ReadAllText(WindowSettingsFile)));
      }
      else settings = new DomainWindowSettings {
        WindowW = 1024,
        WindowH = 768,

        WindowX = 100,
        WindowY = 100,
      };

      return settings;
    }

    public async Task SaveWindowSettingsAsync(DomainWindowSettings settings) {
      string json = await Task.Run(() => JsonConvert.SerializeObject(settings, Formatting.Indented));

      if (!await Task.Run(() => File.Exists(WindowSettingsFile))) await Task.Run(() => Directory.CreateDirectory(Path.GetDirectoryName(WindowSettingsFile)));

      await Task.Run(() => File.WriteAllText(WindowSettingsFile, json));
    }

    #endregion IWindowSettingsRepository Implementation

  }

}
