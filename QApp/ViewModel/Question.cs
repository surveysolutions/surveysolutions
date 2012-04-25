using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;

namespace QApp.ViewModel
{
    public class QuestionData : ModuleData
    {
        private CompleteQuestionView question;

        public QuestionData()
        {
        }

        public QuestionData(CompleteQuestionView question)
        {
            Question = question;
        }


        public CompleteQuestionView Question
        {
            get { return question; }
            private set { SetValue<CompleteQuestionView>("Question", ref question, value); }
        }

        public override void Load()
        {
            base.Load();
            if (Question == null) return;
        }

    }
    public class Question : Module
    {
         public override void InitData(object parameter) {
            base.InitData(parameter);

             var item = parameter as CompleteQuestionView;
             if (item != null)
             {
                 Data = new QuestionData(item);

                 foreach (var completeAnswerView in QuestionData.Question.Answers)
                 {
                     if (completeAnswerView.Selected)
                     {
                         SelectedAnswer = completeAnswerView;
                     }
                 }
             }
         }


         public QuestionData QuestionData { get { return (QuestionData)Data; } }


         private CompleteAnswerView selectedAnswer;
         public CompleteAnswerView SelectedAnswer
         {
             get { return selectedAnswer; }
             set { SetValue<CompleteAnswerView>("SelectedAnswer", ref selectedAnswer, value); }
         }

        /* void RaiseSelectedAnswerChanged(CompleteAnswerView oldValue, CompleteAnswerView newValue) {
            Detail = (AgentDetail)ModulesManager.CreateModule(Detail, new AgentDetailData(newValue), this);
*/


         #region Commands
         protected override void InitializeCommands()
         {
             base.InitializeCommands();
             SetCurrentAnswerCommand = new SimpleActionCommand(DoSetCurrentAnswer);
         }


        /* void RaiseCurrentGroupChanged(CompleteGroupViewV oldValue, CompleteGroupViewV newValue)
         {
             GroupDetail = (GroupDetail)ModulesManager.CreateModule(GroupDetail, new GroupDetailData(newValue), this, newValue);
         }*/

         void DoSetCurrentAnswer(object p)
         {
             //bad approach!!!
             //reload current data
             //TODO: !!!
             var answer = p as CompleteAnswerView;

             if (answer != null)
             {

                 foreach (var completeAnswerView in QuestionData.Question.Answers)
                 {
                     if (completeAnswerView.PublicKey != answer.PublicKey)
                         completeAnswerView.Selected = false;
                     else
                     {
                         completeAnswerView.Selected = true;
                     }
                 }
                 SelectedAnswer = answer;
 
                 var command =new UpdateAnswerInCompleteQuestionnaireCommand(QuestionData.Question.QuestionnaireId,
                                                                                          new CompleteAnswerView[] { answer },
                                                                                          null/*add propogation later*/,
                                                                                          new UserLight("0", "system"));

                 var commandInvoker = Initializer.Kernel.Get<ICommandInvoker>();
                 commandInvoker.Execute(command);

             }
             
             /*if (currentGroup != null)
             {
                 Data = new CompletedQuestionnaireData(completedQuestionnaireId, currentGroup.PublicKey);
                 (Data as CompletedQuestionnaireData).Load();
             }


             CurrentGroup = CompletedQuestionnaireData.CompleteQuestionnaireItem.CurrentGroup;*/
         }


         public ICommand SetCurrentAnswerCommand { get; private set; }



         #endregion



    }
}
