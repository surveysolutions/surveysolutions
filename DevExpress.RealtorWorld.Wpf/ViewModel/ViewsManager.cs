using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public abstract class ViewsManager {
        public static ViewsManager Current { get; private set; }
        public ViewsManager() {
            Current = this;
        }
        public abstract void CreateView(Module module);
        public abstract void ShowView(Module module);
    }
}
