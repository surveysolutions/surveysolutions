using System.Windows.Input;
using RavenQuestionnaire.Core;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace QApp.ViewModel
{
    public class CompletedQuestionnairesData : ModuleData
    {
        public override void Load()
        {
            base.Load();
            //replace with injections
            var viewRepository = new ViewRepository(Initializer.Kernel);
            CompleteQuestionnaires = viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(new CQGroupedBrowseInputModel());
        }


        public CQGroupedBrowseView CompleteQuestionnaires { get; private set; }

    }

    public class CompletedQuestionnaires : ModuleWithNavigator
    {


        public CompletedQuestionnaires()
        {
        }

        public override void InitData(object parameter)
        {
            base.InitData(parameter);

        }

        public override void SaveData()
        {
            base.SaveData();

        }

        #region Commands

        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            OpenQuestionnaireCommand = new SimpleActionCommand(DoOpenQuestionnaire);
            //delete///
            ShowCompletedItemCommand=new SimpleActionCommand(DoShowCompletedItemCommand);
        }

        private void DoOpenQuestionnaire(object p)
        {
            CompleteQuestionnaireBrowseItem item = p as CompleteQuestionnaireBrowseItem;
            //Parent.
            //CurrentAgent = p as Agent;
        }

        public ICommand OpenQuestionnaireCommand { get; private set; }

        //*need to delete//

        public ICommand ShowCompletedItemCommand { get; private set; }

        private void DoShowCompletedItemCommand(object p)
        {
            CompleteQuestionnaireBrowseItem item = p as CompleteQuestionnaireBrowseItem;
            //Parent.
            //CurrentAgent = p as Agent;
        }

        #endregion

    }
}
