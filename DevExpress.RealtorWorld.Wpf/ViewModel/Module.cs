using System;
using DevExpress.RealtorWorld.Xpf.Helpers;
using System.Collections.Generic;
using System.Threading;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class ModuleData : BindingAndDisposable {
        public virtual void Load() { }
    }
    public class Module : BindingAndDisposable {
        ModuleData data;
        object view;
        Module parent;
        bool isVisible;

        public Module() {
            Loaded = new AutoResetEvent(false);
        }
        public bool IsPersistentModule { get; protected set; }
        public ModuleData Data {
            get { return data; }
            set { SetValue<ModuleData>("Data", ref data, value); }
        }
        public object View {
            get { return view; }
            set { SetValue<object>("View", ref view, value); }
        }
        public Module Parent {
            get { return parent; }
            set { SetValue<Module>("Parent", ref parent, value); }
        }
        public bool IsVisible {
            get { return isVisible; }
            set { SetValue<bool>("IsVisible", ref isVisible, value); }
        }
        public AutoResetEvent Loaded { get; private set; }
        public virtual List<Module> GetSubmodules() {
            return new List<Module>();
        }
        public virtual void InitData(object parameter) { }
        public virtual void SaveData() { }
        protected override void DisposeManaged() {
            Parent = null;
            base.DisposeManaged();
        }
    }
}
