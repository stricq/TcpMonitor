using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;

using AutoMapper;

using Str.Common.Extensions;

using Str.MvvmCommon.Contracts;
using Str.MvvmCommon.Core;

using TcpMonitor.Domain.Contracts;


namespace TcpMonitor.Wpf {

  internal sealed partial class App : Application {

    #region Private Fields

    private readonly IMvvmContainer container;

    #endregion Private Fields

    #region Constructor

    public App() {
      container = new MvvmContainer();

      container.Initialize(() => new AggregateCatalog(new DirectoryCatalog(Directory.GetCurrentDirectory(), "TcpMonitor.dll"),
                                                      new DirectoryCatalog(Directory.GetCurrentDirectory(), "TcpMonitor.*.dll"),
                                                      new DirectoryCatalog(Directory.GetCurrentDirectory(), "Str.*.dll")));
    }

    #endregion Constructor

    #region Overrides

    protected override void OnStartup(StartupEventArgs e) {
      TaskHelper.RunOnUiThread(() => { }).FireAndForget(); // Initialize the synchronization context.

      try {
        IEnumerable<IAutoMapperConfiguration> configurations = container.GetAll<IAutoMapperConfiguration>();

        MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg => configurations.ForEach(configuration => configuration.RegisterMappings(cfg)));

        try {
          mapperConfiguration.AssertConfigurationIsValid();
        }
        catch(Exception ex) {
          MessageBox.Show(ex.Message, "Mapping Validation Error");
        }

        container.RegisterInstance(mapperConfiguration.CreateMapper());

        container.InitializeControllers();
      }
      catch(Exception ex) {
        while(ex.InnerException != null) ex = ex.InnerException;

        MessageBox.Show(ex.Message, "MEF Error");
      }

      base.OnStartup(e);
    }

    #endregion Overrides

    }

}
