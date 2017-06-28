using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using TcpMonitor.Wpf.ViewEntities;


namespace TcpMonitor.Wpf.Extensions {

  public static class ObservableCollection {

    public static void Sort(this ObservableCollection<ConnectionViewEntity> list, IComparer<ConnectionViewEntity> comparer) {
      List<ConnectionViewEntity> sorted = list.OrderBy(item => item, comparer).ToList();

      sorted.ForEach(item => {
        if (list.IndexOf(item) == sorted.IndexOf(item)) return;

        bool isSelected = item.IsSelected;

        list.Move(list.IndexOf(item), sorted.IndexOf(item));

        item.IsSelected = isSelected;
      });
    }

  }

}
