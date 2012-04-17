using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class StatisticsData : ModuleData {
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
    public class Statistics : ModuleWithNavigator {
        static Home savedCurrentHome;
        Home currentHome;
        HomeStatistic statistic;

        public Statistics() {
            Title = "Statistics";
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            Home neededHome = parameter as Home;
            if(neededHome == null)
                neededHome = savedCurrentHome;
            CurrentHome = neededHome == null ? StatisticsData.Homes[0] : neededHome;
        }
        public override void SaveData() {
            base.SaveData();
            savedCurrentHome = CurrentHome;
        }
        public override List<Module> GetSubmodules() {
            List<Module> submodules = base.GetSubmodules();
            submodules.Add(Statistic);
            return submodules;
        }
        public StatisticsData StatisticsData { get { return (StatisticsData)Data; } }
        public Home CurrentHome {
            get { return currentHome; }
            set { SetValue<Home>("CurrentHome", ref currentHome, value, RaiseCurrentHomeChanged); }
        }
        public HomeStatistic Statistic {
            get { return statistic; }
            private set { SetValue<HomeStatistic>("Statistic", ref statistic, value); }
        }
        void RaiseCurrentHomeChanged(Home oldValue, Home newValue) {
            Statistic = (HomeStatistic)ModulesManager.CreateModule(Statistic, new HomeStatisticData(newValue), this);
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
