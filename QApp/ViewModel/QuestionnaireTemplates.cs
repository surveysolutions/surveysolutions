using System.Windows.Input;
using QApp.Heritage;
using RavenQuestionnaire.Core;
using System.Collections.ObjectModel;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace QApp.ViewModel 
{
    public class QuestionnaireTemplatesData : ModuleData 
    {
        public override void Load()
        {
            base.Load();

            //replace with injections
            ViewRepository vr = new ViewRepository(Initializer.Kernel);
            var AllQuestionnaires = vr.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel());
            Items = new ObservableCollection<QuestionnaireBrowseItem>();
            foreach (var questionnaireBrowseItem in AllQuestionnaires.Items)
            {
                Items.Add(questionnaireBrowseItem);
            }
        }

        public ObservableCollection<QuestionnaireBrowseItem> Items { private set; get; }
        
    }

    public class QuestionnaireTemplates : ModuleWithNavigator {


        public QuestionnaireTemplates()
        {
        }

        public override void InitData(object parameter) {
            base.InitData(parameter);
           
        }
        public override void SaveData() {
            base.SaveData();
            
        }

        #region Commands

        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            CreateQuestionnaireCommand = new SimpleActionCommand(DoCreateQuestionnaire);
        }
        void DoCreateQuestionnaire(object p)
        {
            //Parent.ShowCompletedItemCommand();
        }
        public ICommand CreateQuestionnaireCommand { get; private set; }

        #endregion
    }
}
