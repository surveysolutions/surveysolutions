using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class ListingData : ModuleData {
        ObservableCollection<Home> homes;

        public override void Load() {
            base.Load();
            Homes = new ObservableCollection<Home>();
            foreach(Home home in DataSource.Current.GetHomes()) {
                Homes.Add(home);
            }
        }
        public ObservableCollection<Home> Homes {
            get { return homes; }
            set { SetValue<ObservableCollection<Home>>("Homes", ref homes, value); }
        }
    }
    public class Listing : ModuleWithNavigator {
        static Home savedCurrentHome;
        Home currentHome;
        HomeDetail detail;

        public Listing() {
            Title = "Listings";
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            Home neededHome = parameter as Home;
            if(neededHome == null)
                neededHome = savedCurrentHome;
            CurrentHome = neededHome == null ? ListingData.Homes[0] : neededHome;
        }
        public override void SaveData() {
            base.SaveData();
            savedCurrentHome = CurrentHome;
        }
        public override List<Module> GetSubmodules() {
            List<Module> submodules = base.GetSubmodules();
            submodules.Add(Detail);
            return submodules;
        }
        public ListingData ListingData { get { return (ListingData)Data; } }
        public Home CurrentHome {
            get { return currentHome; }
            set { SetValue<Home>("CurrentHome", ref currentHome, value, RaiseCurrentHomeChanged); }
        }
        public HomeDetail Detail {
            get { return detail; }
            private set { SetValue<HomeDetail>("Detail", ref detail, value); }
        }
        void RaiseCurrentHomeChanged(Home oldValue, Home newValue) {
            Detail = (HomeDetail)ModulesManager.CreateModule(Detail, new HomeDetailData(newValue), this);
        }
        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();
            SetCurrentHomeCommand = new SimpleActionCommand(DoSetCurrentHome);
        }
        public ICommand SetCurrentHomeCommand { get; private set; }
        void DoSetCurrentHome(object p) { CurrentHome = p as Home; }
        #endregion
    }
}
