using Ninject;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RavenQuestionnaire.Core;
using DevExpress.RealtorWorld.Xpf.Helpers;
using RavenQuestionnaire.Core.Views.Answer;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;

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
         }

         public QuestionData QuestionData { get { return (QuestionData)Data; } }

         private CompleteAnswerView selectedAnswer;
         public CompleteAnswerView SelectedAnswer
         {
             get { return selectedAnswer; }
             set { SetValue<CompleteAnswerView>("SelectedAnswer", ref selectedAnswer, value); }
         }

         #region Commands
         protected override void InitializeCommands()
         {
             base.InitializeCommands();
             SetCurrentAnswerCommand = new SimpleActionCommand(DoSetCurrentAnswer);
             CloseWindowCommand = new SimpleActionCommand(DoClose);
         }

        private void DoClose(object obj)
        {
            //BAD!
            //don't do this
            //open in main window
            
            var singleOrDefault = Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive);
            if (singleOrDefault != null)
                singleOrDefault.Close();
            //window.Close;
        }
        
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
                         if (QuestionData.Question.QuestionType == QuestionType.MultyOption)
                             if (completeAnswerView.PublicKey == answer.PublicKey)
                             completeAnswerView.Selected = !completeAnswerView.Selected;
                         else
                            completeAnswerView.Selected = completeAnswerView.PublicKey == answer.PublicKey;
                 }
                 SelectedAnswer = answer;
                 var command = new UpdateAnswerInCompleteQuestionnaireCommand(QuestionData.Question.QuestionnaireId,
                                                                              new CompleteAnswerView[] { answer },
                                                                              null /*add propogation later*/,
                                                                              new UserLight("0", "system"));
                 var commandInvoker = Initializer.Kernel.Get<ICommandInvoker>();
                 commandInvoker.Execute(command);
             }
             else
             {
                 var question = p as CompleteQuestionView;
                 if (question!=null)
                 {
                     foreach (CompleteAnswerView answerView in question.Answers)
                         answerView.QuestionId = question.PublicKey;
                     var command = new UpdateAnswerInCompleteQuestionnaireCommand(question.QuestionnaireId,
                                                                                  question.Answers, null,
                                                                                  new UserLight("0", "system"));
                     var commandInvoker = Initializer.Kernel.Get<ICommandInvoker>();
                     commandInvoker.Execute(command);
                 }
             }
            UpdateCurrentDataQuestion();
        }

        public ICommand SetCurrentAnswerCommand { get; private set; }

        public ICommand CloseWindowCommand { get; private set; }

        #endregion

        #region Private Method

        ////bad approach!!!
        //reload current data and add async update in database
        private void UpdateCurrentDataQuestion()
        {
            ViewRepository viewRepository = new ViewRepository(Initializer.Kernel);
            var test =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(QuestionData.Question.QuestionnaireId) { CurrentGroupPublicKey = QuestionData.Question.GroupPublicKey });
            var item = new CompleteQuestionView();
            foreach (CompleteQuestionView cqv in test.CurrentGroup.Groups[0].Questions.Where(cqv => cqv.PublicKey == QuestionData.Question.PublicKey))
                item = cqv;
            if (item != null)
            {
                Data = new QuestionData(item);
                foreach (var completeAnswerView in QuestionData.Question.Answers)
                    if (completeAnswerView.Selected)
                        SelectedAnswer = completeAnswerView;
            }
        }

        #endregion

    }
}
