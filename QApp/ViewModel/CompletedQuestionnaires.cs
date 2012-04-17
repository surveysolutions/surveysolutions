using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace QApp.ViewModel {
    public class CompletedQuestionnairesData : ModuleData {
        
       

        public override void Load() {
            base.Load();


            //replace with injections
            ViewRepository viewRepository = new ViewRepository(Initializer.Kernel);

            CompleteQuestionnaires = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>
                (new CompleteQuestionnaireBrowseInputModel(){PageSize = 100});
            
        }


        public CompleteQuestionnaireBrowseView CompleteQuestionnaires
        { 
            get;
            private set; 
        }
        
    }
    public class CompletedQuestionnaires : ModuleWithNavigator {


        public CompletedQuestionnaires()
        {
        }

        public override void InitData(object parameter) {
            base.InitData(parameter);

            string item = parameter as string;
           
        }
        public override void SaveData() {
            base.SaveData();
            
        }

         #region Commands
         protected override void InitializeCommands() {
             base.InitializeCommands();
             OpenQuestionnaireCommand = new SimpleActionCommand(DoOpenQuestionnaire);
         }
         void DoOpenQuestionnaire(object p)
         {
             CompleteQuestionnaireBrowseItem item = p as CompleteQuestionnaireBrowseItem;
             //Parent.
             //CurrentAgent = p as Agent;
         }
         public ICommand OpenQuestionnaireCommand { get; private set; }
         #endregion
  
        
    }
}
