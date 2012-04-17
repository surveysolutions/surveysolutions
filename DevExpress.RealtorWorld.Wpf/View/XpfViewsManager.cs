using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using System.Collections.Generic;

namespace DevExpress.RealtorWorld.Xpf.View {
    public class XpfViewsManager : ViewsManager {
        Dictionary<Type, Type> views = new Dictionary<Type, Type>();

        public void RegisterView(Type moduleType, Type viewType) {
            if(!moduleType.IsSubclassOf(typeof(Module)))
                throw new ArgumentOutOfRangeException("moduleType");
            if(!viewType.IsSubclassOf(typeof(ModuleView)))
                throw new ArgumentOutOfRangeException("viewType");
            views.Add(moduleType, viewType);
        }
        public override void CreateView(Module module) {
            Type viewType = views[module.GetType()];
            ModuleView view = (ModuleView)Activator.CreateInstance(viewType);
            view.Opacity = 0.0;
            module.View = view;
            view.DataContext = module;
        }
        public override void ShowView(Module module) {
            ModuleView view = (ModuleView)module.View;
            view.Hide += OnViewHide;
            view.Clear += OnViewClear;
            view.IsVisibleChanged += OnViewIsVisibleChanged;
            view.Opacity = 1.0;
            view.RaiseReady();
        }
        void OnViewIsVisibleChanged(object sender, EventArgs e) {
            ModuleView view = (ModuleView)sender;
            Module module = view.DataContext as Module;
            if(module != null)
                module.IsVisible = view.IsVisible;
        }
        void OnViewHide(object sender, EventArgs e) {
            ModuleView view = (ModuleView)sender;
            Module module = view.DataContext as Module;
            if(module != null) {
                foreach(Module submodule in module.GetSubmodules()) {
                    if(submodule == null) continue;
                    submodule.SaveData();
                }
                module.SaveData();
            }
        }
        void OnViewClear(object sender, EventArgs e) {
            ModuleView view = (ModuleView)sender;
            Module module = view.DataContext as Module;
            if(module != null && module.IsPersistentModule) return;
            view.IsVisibleChanged -= OnViewIsVisibleChanged;
            view.Hide -= OnViewHide;
            view.Clear -= OnViewClear;
            view.DataContext = null;
            if(module != null) {
                foreach(Module submodule in module.GetSubmodules()) {
                    if(submodule == null) continue;
                    ModuleView subview = (ModuleView)submodule.View;
                    subview.OnClear();
                }
                module.View = null;
                module.Dispose();
            }
            ContentControl cc = view.Parent as ContentControl;
            if(cc != null)
                cc.Content = null;
            ContentPresenter cp = view.Parent as ContentPresenter;
            if(cp != null)
                cp.Content = null;
        }
    }
}
