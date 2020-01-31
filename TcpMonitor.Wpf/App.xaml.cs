using System;
using System.Windows;

using AutoMapper;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Str.DialogView.Extensions;
using Str.MvvmCommon.Contracts;
using Str.MvvmCommon.Core;

using TcpMonitor.Domain.Contracts;

using TcpMonitor.Repository.Mappings;
using TcpMonitor.Repository.Repositories;
using TcpMonitor.Repository.Services;

using TcpMonitor.Wpf.Controllers;
using TcpMonitor.Wpf.Mappings;
using TcpMonitor.Wpf.ViewModels;
using TcpMonitor.Wpf.Views;


namespace TcpMonitor.Wpf {

  internal sealed partial class App : Application {

    #region Private Fields

    private readonly IMvvmContainer container;

    #endregion Private Fields

    #region Constructor

    public App() {
      container = new MvvmContainer();

      container.Initialize(ConfigureServices);
    }

    #endregion Constructor

    #region Overrides

    protected override void OnStartup(StartupEventArgs e) {
      container.OnStartup();

      try {
        container.InitializeControllers();
      }
      catch(Exception ex) {
        while(ex.InnerException != null) ex = ex.InnerException;

        MessageBox.Show(ex.Message, "Dependency Injection Error");
      }

      container.Get<TcpMonitorView>().Show();

      base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs args) {
      container.OnExit();

      base.OnExit(args);
    }

    #endregion Overrides

    #region Private Methods

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
      services.AddAutoMapper(typeof(ViewEntityMappingConfiguration), typeof(DomainModelMappingConfiguration));

      services.AddStrDialogView();

      services.AddSingleton<TcpMonitorView>();

      services.AddSingleton<IController, ConnectionsController>();
      services.AddSingleton<ConnectionsViewModel>();

      services.AddSingleton<IController, TcpMonitorController>();
      services.AddSingleton<TcpMonitorViewModel>();

      services.AddSingleton<IWindowSettingsRepository, SettingsRepository>();

      services.AddSingleton<IConnectionsService, ConnectionsService>();

      services.AddSingleton<ICapturePackets, PacketCaptureService>();
    }

    #endregion Private Methods

  }

}
