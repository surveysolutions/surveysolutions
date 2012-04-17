using System;
using System.Collections.Generic;

namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class ModuleWithNavigator : Module {
        string title;
        Navigator navigator;

        public override void InitData(object parameter) {
            base.InitData(parameter);
            Navigator = (Navigator)ModulesManager.CreateModule(null, new NavigatorData(), this);
        }
        public override List<Module> GetSubmodules() {
            List<Module> submodules = base.GetSubmodules();
            submodules.Add(Navigator);
            return submodules;
        }
        public string Title {
            get { return title; }
            set { SetValue<string>("Title", ref title, value); }
        }
        public Navigator Navigator {
            get { return navigator; }
            set { SetValue<Navigator>("Navigator", ref navigator, value); }
        }
    }
}
