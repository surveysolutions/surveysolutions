using System.Windows.Input;
using RavenQuestionnaire.Core;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

namespace QApp.ViewModel
{
    public class CompletedQuestionnairesData : ModuleData
    {
        public override void Load()
        {
            base.Load();
            //replace with injections
            var viewRepository = new ViewRepository(Initializer.Kernel);
            //CompleteQuestionnaires = viewRepository.Load
            //    <CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>
            //    (new CompleteQuestionnaireBrowseInputModel() {PageSize = 100});
            CompleteQuestionnaires = viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(new CQGroupedBrowseInputModel());
            var str = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    new CompleteQuestionnaireViewInputModel("0dedde4e-02a0-41aa-90b8-4e65ff4bd93d"));
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
        }

        private void DoOpenQuestionnaire(object p)
        {
            CompleteQuestionnaireBrowseItem item = p as CompleteQuestionnaireBrowseItem;
            //Parent.
            //CurrentAgent = p as Agent;
        }

        public ICommand OpenQuestionnaireCommand { get; private set; }

        #endregion

    }
}
