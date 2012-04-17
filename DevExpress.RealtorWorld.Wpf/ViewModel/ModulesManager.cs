using System;
using DevExpress.RealtorWorld.Xpf.Helpers;
using System.Collections.Generic;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public static class ModulesManager {
        class ModuleCreator {
            Module module;
            ModuleData data;
            object parameter;

            public ModuleCreator(Module module, ModuleData data, object parameter) {
                this.module = module;
                this.data = data;
                this.parameter = parameter;
            }
            public void InitModule() {
                module.Data = data;
                module.InitData(parameter);
                List<Module> submodules = module.GetSubmodules();
                BackgroundHelper.DoInBackground(WaitSubmodules, ShowModule);
            }
            void WaitSubmodules() {
                foreach(Module submodule in module.GetSubmodules()) {
                    if(submodule == null) continue;
                    submodule.Loaded.WaitOne();
                }
            }
            void ShowModule() {
                WaitScreenHelperBase.Current.DisableWaitScreens();
                ViewsManager.Current.ShowView(module);
                module.Loaded.Set();
            }
        }
        static Dictionary<Type, Type> moduleTypes = new Dictionary<Type, Type>();

        public static void RegisterModule(Type dataType, Type moduleType) {
            if(!dataType.IsSubclassOf(typeof(ModuleData)))
                throw new ArgumentOutOfRangeException("dataType");
            if(!moduleType.IsSubclassOf(typeof(Module)))
                throw new ArgumentOutOfRangeException("moduleType");
            moduleTypes.Add(dataType, moduleType);
        }
        public static Module CreateModule(Module module, ModuleData data, Module parent, object parameter = null) {
            WaitScreenHelperBase.Current.EnableWaitScreens();
            if(module == null) {
                Type moduleType = moduleTypes[data.GetType()];
                module = (Module)Activator.CreateInstance(moduleType);
                module.Parent = parent;
                ViewsManager.Current.CreateView(module);
            }
            ModuleCreator creator = new ModuleCreator(module, data, parameter);
            BackgroundHelper.DoInBackground(data.Load, creator.InitModule);
            return module;
        }
    }
}
