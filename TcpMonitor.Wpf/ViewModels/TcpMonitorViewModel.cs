using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

using STR.MvvmCommon;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels {

  [Export]
  [ViewModel(nameof(TcpMonitorViewModel))]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public class TcpMonitorViewModel : ObservableObject {

    #region PrivateFields

    private RelayCommand<EventArgs> initialized;

    private RelayCommandAsync<RoutedEventArgs> loaded;

    private RelayCommand<CancelEventArgs> closing;

    private WindowSettingsViewEntity settings;

    #endregion PrivateFields

    #region Properties

    public RelayCommand<EventArgs> Initialized {
      get => initialized;
      set { SetField(ref initialized, value, () => Initialized); }
    }

    public RelayCommandAsync<RoutedEventArgs> Loaded {
      get => loaded;
      set { SetField(ref loaded, value, () => Loaded); }
    }

    public RelayCommand<CancelEventArgs> Closing {
      get => closing;
      set { SetField(ref closing, value, () => Closing); }
    }

    public WindowSettingsViewEntity Settings {
      get => settings;
      set { SetField(ref settings, value, () => Settings); }
    }

    #endregion Properties

  }

}
