using System;
using System.Collections.Generic;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.Extensions {

  public class ConnectionViewEntityComparer : IComparer<ConnectionViewEntity> {

    public int Compare(ConnectionViewEntity x, ConnectionViewEntity y) {
      if (x == null || y == null) return 0;

      int compared = String.Compare(x.ProcessName, y.ProcessName, StringComparison.OrdinalIgnoreCase);

      if (compared == 0) compared = x.LocalPort.CompareTo(y.LocalPort);

      if (compared == 0) compared = x.RemotePort.CompareTo(y.RemotePort);

      return compared;
    }

  }

}
