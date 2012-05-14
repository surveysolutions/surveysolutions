using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;

namespace QApp.ViewModel {
  
    public class MainScreenData : ModuleData {}


    public class MainScreen : Module {
      
        IEnumerable<ListingTileData> listingTileDataSource;
        bool animateListingTileContent;
        bool animateAgentsTileContent;
        Type currentModuleDataType;
        Module currentModule;

        public MainScreen() {
            IsPersistentModule = true;
            ShowModule<MainScreenData>(null);
        }
        public override void InitData(object parameter) {
            base.InitData(parameter);
            
        }
        public MainScreenData MainData { get { return (MainScreenData)Data; } }



        public void ShowModule<T>(object parameter) where T : ModuleData, new() {
            CurrentModuleDataType = typeof(T);
            if (typeof(T) == typeof(MainScreenData))
                CurrentModule = this;
             else
                CurrentModule = typeof(T) == typeof(QuestionData) ? 
                        ModulesManager.CreateModule(null, new QuestionData(parameter as CompleteQuestionView) , this, parameter) : 
                        ModulesManager.CreateModule(null, new T(), this, parameter);
            
        }
        public IEnumerable<ListingTileData> ListingTileDataSource {
            get { return listingTileDataSource; }
            private set { SetValue<IEnumerable<ListingTileData>>("ListingTileDataSource", ref listingTileDataSource, value); }
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
       
      
        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();

            ShowMainCommand = new ExtendedActionCommand(DoShowModule<MainScreenData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(MainScreenData));
            ShowMainScreenCommand = new SimpleActionCommand(DoShowModule<MainScreenData>);
            
            ShowTakeNewCommand = new ExtendedActionCommand(DoShowModule<QuestionnaireTemplatesData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionnaireTemplatesData));
            ShowCompletedItemsCommand = new ExtendedActionCommand(DoShowModule<CompletedQuestionnairesData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(CompletedQuestionnairesData));
            ShowCompletedItemCommand = new ExtendedActionCommand(DoShowModule<QuestionnaireDetailData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionnaireDetailData));

            
            //ShowModalQuestionCommand = new ExtendedActionCommand(DoShowModuleModal<QuestionData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionData));
            ShowModalQuestionCommand = new ExtendedActionCommand(DoShowModule<QuestionData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionData));

            CreateNewCompletedAndOpen = new SimpleActionCommand(CreateNew);

        }



        void DoShowModuleModal<T>(object p) where T : ModuleData, new()
        {
            T data = new T();
            var module = ModulesManager.CreateModule(null, data, this, p);

            DXWindow _window = new DXWindow();
            _window.Content = module.View;
            _window.ShowDialog();
        }


        private void CreateNew(object id)
        {

            string qId = id as string;
            if (String.IsNullOrEmpty(qId))
                return;
            var command = new CreateNewCompleteQuestionnaireCommand(qId,
                                                                    new UserLight("0", "system"), 
                                                                    new SurveyStatus(Guid.NewGuid()), 
                                                                    new UserLight("0", "system"));
            var commandInvoker = Initializer.Kernel.Get<ICommandInvoker>();
            commandInvoker.Execute(command);

            id = command.CompleteQuestionnaireId;
            
            if (ShowCompletedItemCommand.CanExecute(id))
                ShowCompletedItemCommand.Execute(id);
        }

        public ICommand ShowMainCommand { get; private set; }

        public ICommand ShowMainScreenCommand { get; private set; }
        public ICommand ShowTakeNewCommand { get; private set; }
        public ICommand ShowCompletedItemsCommand { get; private set; }
        public ICommand ShowCompletedItemCommand { get; private set; }


        public ICommand CreateNewCompletedAndOpen { get; private set; }

        public ICommand ShowModalQuestionCommand { get; private set; }

        public ICommand CloseModalCommand { get; private set; }

        bool AllowSwitchToTheModule(object moduleDataType) {
            return CurrentModuleDataType != moduleDataType as Type;
        }
        void DoShowModule<T>(object p) where T : ModuleData, new() { ShowModule<T>(p); }
        #endregion
    }
}
