using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using AutoMapper;

using Str.Common.Extensions;
using Str.Common.Messages;

using Str.MvvmCommon.Contracts;
using Str.MvvmCommon.Core;

using TcpMonitor.Domain.Contracts;
using TcpMonitor.Domain.Models;

using TcpMonitor.Wpf.ViewEntities;
using TcpMonitor.Wpf.ViewModels;


namespace TcpMonitor.Wpf.Controllers {

  [Export(typeof(IController))]
  public class TcpMonitorController : IController {

    #region Private Fields

    private bool isStartupComplete;

    private readonly TcpMonitorViewModel viewModel;

    private readonly IMessenger messenger;
    private readonly IMapper    mapper;

    private readonly IWindowSettingsRepository settingsRepository;

    #endregion Private Fields

    #region Constructor

    [ImportingConstructor]
    public TcpMonitorController(TcpMonitorViewModel viewModel, IMessenger messenger, IMapper mapper, IWindowSettingsRepository settingsRepository) {
      if (Application.Current != null) Application.Current.DispatcherUnhandledException += OnCurrentDispatcherUnhandledException;

      AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

      Dispatcher.CurrentDispatcher.UnhandledException += OnCurrentDispatcherUnhandledException;

      TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

      this.viewModel = viewModel;

      this.messenger = messenger;
         this.mapper = mapper;

      this.settingsRepository = settingsRepository;
    }

    #endregion Constructor

    #region IController Implementation

    public int InitializePriority { get; } = 1000;

    public async Task InitializeAsync() {
      viewModel.Settings = mapper.Map<WindowSettingsViewEntity>(await settingsRepository.LoadWindowSettingsAsync().Fire());

      RegisterCommands();
    }

    #endregion IController Implementation

    #region Commands

    private void RegisterCommands() {
      viewModel.Initialized = new RelayCommandAsync<EventArgs>(OnInitializedAsync);
      viewModel.Loaded      = new RelayCommandAsync<RoutedEventArgs>(OnLoadedAsync);
      viewModel.Closing     = new RelayCommandAsync<CancelEventArgs>(OnClosingAsync);
    }

    private Task OnInitializedAsync(EventArgs args) {
      isStartupComplete = true;

      messenger.Send(new ApplicationInitializedMessage());

      return Task.CompletedTask;
    }

    private async Task OnLoadedAsync(RoutedEventArgs args) {
      await messenger.SendAsync(new ApplicationLoadedMessage()).Fire();
    }

    private async Task OnClosingAsync(CancelEventArgs args) {
      ApplicationClosingMessage message = new ApplicationClosingMessage();

      await messenger.SendAsync(message).Fire();

      if (!message.Cancel && viewModel.Settings.AreSettingsChanged) await SaveSettingsAsync().Fire();

      args.Cancel = message.Cancel;
    }

    #endregion Commands

    #region Private Methods

    private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
      if (!(e.ExceptionObject is Exception ex)) return;

      if (e.IsTerminating) MessageBox.Show(ex.Message, "Fatal Domain Unhandled Exception");
      else messenger.SendOnUiThreadAsync(new ApplicationErrorMessage { HeaderText = "Domain Unhandled Exception", Exception = ex });
    }

    private void OnCurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
      if (e.Exception == null) return;

      if (isStartupComplete) {
        messenger.SendOnUiThreadAsync(new ApplicationErrorMessage { HeaderText = "Dispatcher Unhandled Exception", Exception = e.Exception });

        e.Handled = true;
      }
      else {
        e.Handled = true;

        MessageBox.Show(e.Exception.Message, "Fatal Dispatcher Exception");

        Application.Current.Shutdown();
      }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
      if (e.Exception == null || e.Exception.InnerExceptions.Count == 0) return;

      foreach(Exception ex in e.Exception.InnerExceptions) {
        if (isStartupComplete) {
          messenger.SendOnUiThreadAsync(new ApplicationErrorMessage { HeaderText = "Unobserved Task Exception", Exception = ex });
        }
        else {
          MessageBox.Show(ex.Message, "Fatal Unobserved Task Exception");
        }
      }

      if (!isStartupComplete) Application.Current.Shutdown();

      e.SetObserved();
    }

    private async Task SaveSettingsAsync() {
      await settingsRepository.SaveWindowSettingsAsync(mapper.Map<DomainWindowSettings>(viewModel.Settings)).Fire();
    }

    #endregion Private Methods

  }

}
