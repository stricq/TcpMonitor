using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using AutoMapper;

using STR.Common.Messages;

using STR.MvvmCommon;
using STR.MvvmCommon.Contracts;

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
    public TcpMonitorController(TcpMonitorViewModel ViewModel, IMessenger Messenger, IMapper Mapper, IWindowSettingsRepository SettingsRepository) {
      if (Application.Current != null) Application.Current.DispatcherUnhandledException += onCurrentDispatcherUnhandledException;

      AppDomain.CurrentDomain.UnhandledException += onDomainUnhandledException;

      Dispatcher.CurrentDispatcher.UnhandledException += onCurrentDispatcherUnhandledException;

      TaskScheduler.UnobservedTaskException += onUnobservedTaskException;

      System.Windows.Forms.Application.ThreadException += onThreadException;

      viewModel = ViewModel;

      messenger = Messenger;
         mapper = Mapper;

      settingsRepository = SettingsRepository;
    }

    #endregion Constructor

    #region IController Implementation

    public int InitializePriority { get; } = 1000;

    public async Task InitializeAsync() {
      viewModel.Settings = mapper.Map<WindowSettingsViewEntity>(await settingsRepository.LoadWindowSettingsAsync());

      registerCommands();
    }

    #endregion IController Implementation

    #region Commands

    private void registerCommands() {
      viewModel.Initialized = new RelayCommand<EventArgs>(onInitialized);
      viewModel.Loaded      = new RelayCommand<RoutedEventArgs>(onLoaded);
      viewModel.Closing     = new RelayCommand<CancelEventArgs>(onClosing);
    }

    private void onInitialized(EventArgs args) {
      isStartupComplete = true;

      messenger.Send(new ApplicationInitializedMessage());
    }

    private void onLoaded(RoutedEventArgs args) {
      messenger.Send(new ApplicationLoadedMessage());
    }

    private void onClosing(CancelEventArgs args) {
      ApplicationClosingMessage message = new ApplicationClosingMessage();

      Task.Run(() => messenger.SendAsync(message)).Wait();

      if (!message.Cancel && viewModel.Settings.AreSettingsChanged) Task.Run(saveSettings).Wait();

      args.Cancel = message.Cancel;
    }

    #endregion Commands

    #region Private Methods

    private void onDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
      Exception ex = e.ExceptionObject as Exception;

      if (ex == null) return;

      if (e.IsTerminating) MessageBox.Show(ex.Message, "Fatal Domain Unhandled Exception");
      else messenger.SendUi(new ApplicationErrorMessage { HeaderText = "Domain Unhandled Exception", Exception = ex });
    }

    private void onCurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
      if (e.Exception == null) return;

      if (isStartupComplete) {
        messenger.SendUi(new ApplicationErrorMessage { HeaderText = "Dispatcher Unhandled Exception", Exception = e.Exception });

        e.Handled = true;
      }
      else {
        e.Handled = true;

        MessageBox.Show(e.Exception.Message, "Fatal Dispatcher Exception");

        Application.Current.Shutdown();
      }
    }

    private void onUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
      if (e.Exception == null || e.Exception.InnerExceptions.Count == 0) return;

      foreach(Exception ex in e.Exception.InnerExceptions) {
        if (isStartupComplete) {
          messenger.SendUi(new ApplicationErrorMessage { HeaderText = "Unobserved Task Exception", Exception = ex });
        }
        else {
          MessageBox.Show(ex.Message, "Fatal Unobserved Task Exception");
        }
      }

      if (!isStartupComplete) Application.Current.Shutdown();

      e.SetObserved();
    }

    private void onThreadException(object sender, ThreadExceptionEventArgs e) {
      if (e.Exception == null) return;

      messenger.SendUi(new ApplicationErrorMessage { HeaderText = "Thread Exception", Exception = e.Exception });
    }

    private async Task saveSettings() {
      await settingsRepository.SaveWindowSettingsAsync(mapper.Map<DomainWindowSettings>(viewModel.Settings));
    }

    #endregion Private Methods

  }

}
