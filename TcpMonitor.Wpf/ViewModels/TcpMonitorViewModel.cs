using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

using Str.MvvmCommon.Core;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.ViewModels {

  [Export]
  [ViewModel(nameof(TcpMonitorViewModel))]
  [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
  public class TcpMonitorViewModel : ObservableObject {

    #region PrivateFields

    private RelayCommandAsync<EventArgs> initialized;

    private RelayCommandAsync<RoutedEventArgs> loaded;

    private RelayCommandAsync<CancelEventArgs> closing;

    private WindowSettingsViewEntity settings;

    #endregion PrivateFields

    #region Properties

    public RelayCommandAsync<EventArgs> Initialized {
      get => initialized;
      set { SetField(ref initialized, value, () => Initialized); }
    }

    public RelayCommandAsync<RoutedEventArgs> Loaded {
      get => loaded;
      set { SetField(ref loaded, value, () => Loaded); }
    }

    public RelayCommandAsync<CancelEventArgs> Closing {
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
