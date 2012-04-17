using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.Model;


namespace DevExpress.RealtorWorld.Xpf.ViewModel {
    public class ListingTileData {
        public byte[] Image { get; set; }
        public Home Home { get; set; }
    }
    public class MainData : ModuleData {
        public override void Load() {
            base.Load();
            ListingTileData = new ListingTileData();
            ListingTileData.Home = DataSource.Current.GetHomes()[0];
            ListingTileData.Image = ListingTileData.Home.Photo;
            AgentsTileData = DataSource.Current.GetAgents()[0];
        }
        public ListingTileData ListingTileData { get; private set; }
        public Agent AgentsTileData { get; private set; }
    }
    public class Main : Module {
        int listingTileHomeIndex = 0;
        int listingTilePhotoIndex = 1;
        int agentsTileContentIndex = 0;
        Agent agentsTileData;
        Agent nextAgentsTileData;
        ListingTileData listingTileData;
        ListingTileData nextListingTileData;
        IEnumerable<ListingTileData> listingTileDataSource;
        IEnumerable<Agent> agentsTileDataSource;
        bool animateListingTileContent;
        bool animateAgentsTileContent;
        Type currentModuleDataType;
        Module currentModule;

        public Main() {
            IsPersistentModule = true;
            ShowModule<MainData>(null);
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            this.nextListingTileData = MainData.ListingTileData;
            this.nextAgentsTileData = MainData.AgentsTileData;
            ListingTileDataSource = ListingTileDataSourceCore;
            AgentsTileDataSource = AgentsTileDataSourceCore;
            AnimateAgentsTileContent = true;
            AnimateListingTileContent = true;
        }
        public MainData MainData { get { return (MainData)Data; } }
        public void ShowModule<T>(object parameter) where T : ModuleData, new() {
            CurrentModuleDataType = typeof(T);
            if(typeof(T) == typeof(MainData)) {
                CurrentModule = this;
            } else {
                T data = new T();
                CurrentModule = ModulesManager.CreateModule(null, data, this, parameter);
            }
        }
        public IEnumerable<ListingTileData> ListingTileDataSource {
            get { return listingTileDataSource; }
            private set { SetValue<IEnumerable<ListingTileData>>("ListingTileDataSource", ref listingTileDataSource, value); }
        }
        public IEnumerable<Agent> AgentsTileDataSource {
            get { return agentsTileDataSource; }
            private set { SetValue<IEnumerable<Agent>>("AgentsTileDataSource", ref agentsTileDataSource, value); }
        }
        public bool AnimateListingTileContent {
            get { return animateListingTileContent; }
            private set { SetValue<bool>("AnimateListingTileContent", ref animateListingTileContent, value); }
        }
        public bool AnimateAgentsTileContent {
            get { return animateAgentsTileContent; }
            private set { SetValue<bool>("AnimateAgentsTileContent", ref animateAgentsTileContent, value); }
        }
        IEnumerable<ListingTileData> ListingTileDataSourceCore {
            get {
                while(true) {
                    if(this.nextListingTileData != null) {
                        this.listingTileData = this.nextListingTileData;
                        LoadNextListingTileData();
                    }
                    yield return this.listingTileData;
                }
            }
        }
        IEnumerable<Agent> AgentsTileDataSourceCore {
            get {
                while(true) {
                    if(this.nextAgentsTileData != null) {
                        this.agentsTileData = this.nextAgentsTileData;
                        LoadNextAgentsTileData();
                    }
                    yield return this.agentsTileData;
                }
            }
        }
        public Type CurrentModuleDataType {
            get { return currentModuleDataType; }
            set { SetValue<Type>("CurrentModuleDataType", ref currentModuleDataType, value); }
        }
        public Module CurrentModule {
            get { return currentModule; }
            private set { SetValue<Module>("CurrentModule", ref currentModule, value); }
        }
        public Func<object, string> StoryboardSelector { get { return SelectStoryboard; } }
        string SelectStoryboard(object newModuleView) { return newModuleView == View ? "FromLeft" : "FromRight"; }
        void LoadNextAgentsTileData() {
            this.nextAgentsTileData = null;
            BackgroundHelper.DoInBackground(() => {
                ++agentsTileContentIndex;
                IList<Agent> agents = DataSource.Current.GetAgents();
                if(agents.Count == 0) return;
                CorrectAgentsTileContentIndex(agents.Count);
                this.nextAgentsTileData = agents[agentsTileContentIndex];
            }, null);
        }
        void LoadNextListingTileData() {
            this.nextListingTileData = null;
            BackgroundHelper.DoInBackground(() => {
                IList<Home> homes = DataSource.Current.GetHomes();
                if(homes.Count == 0) return;
                CorrectListingTileHomeIndex(homes.Count);
                ListingTileData data = new ListingTileData() { Home = homes[listingTileHomeIndex] };
                IList<HomePhoto> photos = null;
                if(listingTilePhotoIndex != 0) {
                    photos = DataSource.Current.GetPhotos(data.Home);
                    if(CorrectListingTilePhotoIndex(homes.Count, photos.Count)) {
                        data.Home = homes[listingTileHomeIndex];
                        photos = null;
                    }
                }
                data.Image = listingTilePhotoIndex == 0 ? data.Home.Photo : photos[listingTilePhotoIndex - 1].Photo;
                ++listingTilePhotoIndex;
                this.nextListingTileData = data;
            }, null);
        }
        void CorrectListingTileHomeIndex(int homesCount) {
            listingTileHomeIndex %= homesCount;
        }
        bool CorrectListingTilePhotoIndex(int homesCount, int photosCount) {
            if(listingTilePhotoIndex >= photosCount + 1) {
                listingTilePhotoIndex = 0;
                ++listingTileHomeIndex;
                CorrectListingTileHomeIndex(homesCount);
                return true;
            }
            return false;
        }
        void CorrectAgentsTileContentIndex(int agentsCount) {
            agentsTileContentIndex %= agentsCount;
        }
        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();
            ShowMainCommand = new ExtendedActionCommand(DoShowModule<MainData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(MainData));
            ShowListingCommand = new ExtendedActionCommand(DoShowModule<ListingData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(ListingData));
            ShowAgentsCommand = new ExtendedActionCommand(DoShowModule<AgentsData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(AgentsData));
            ShowZillowCommand = new SimpleActionCommand(DoShowZillow);
            ShowBanksCommand = new ExtendedActionCommand(DoShowModule<BanksData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(BanksData));
            ShowResearchCommand = new SimpleActionCommand(DoShowModule<DraftData>);
            ShowUserManagementCommand = new SimpleActionCommand(DoShowModule<DraftData>);
            ShowSystemInfoCommand = new SimpleActionCommand(DoShowModule<DraftData>);
            ShowStatisticsCommand = new ExtendedActionCommand(DoShowModule<StatisticsData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(StatisticsData));
            ShowLoanCalculatorCommand = new ExtendedActionCommand(DoShowModule<LoanCalculatorData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(LoanCalculatorData));
        }
        public ICommand ShowMainCommand { get; private set; }
        public ICommand ShowListingCommand { get; private set; }
        public ICommand ShowAgentsCommand { get; private set; }
        public ICommand ShowZillowCommand { get; private set; }
        void DoShowZillow(object p) { ShowModule<ZillowData>(p); }
        public ICommand ShowBanksCommand { get; private set; }
        public ICommand ShowResearchCommand { get; private set; }
        public ICommand ShowUserManagementCommand { get; private set; }
        public ICommand ShowSystemInfoCommand { get; private set; }
        public ICommand ShowStatisticsCommand { get; private set; }
        public ICommand ShowLoanCalculatorCommand { get; private set; }
        bool AllowSwitchToTheModule(object moduleDataType) {
            return CurrentModuleDataType != moduleDataType as Type;
        }
        void DoShowModule<T>(object p) where T : ModuleData, new() { ShowModule<T>(p); }
        #endregion
    }
}
