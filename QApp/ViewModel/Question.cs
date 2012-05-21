using Ninject;
using System.Linq;
using System.Windows.Input;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.RealtorWorld.Xpf.Helpers;
using RavenQuestionnaire.Core.Views.Answer;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;

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
    public class Question : ModuleWithNavigator
    {
         public override void InitData(object parameter) {
            base.InitData(parameter);
             var currentQuestion = parameter as CompleteQuestionView;
             SetSelectedAnswer(currentQuestion);
             Navigation = new ObservableCollection<List<NavigationItem>>();
             BuildMenu();

             //bad approach!!!
             //get from current data
            var viewRepository = new ViewRepository(Initializer.Kernel);
            _Questionnaire = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(currentQuestion.QuestionnaireId) { CurrentGroupPublicKey = currentQuestion.GroupPublicKey });
            for (int i = 0; i < _Questionnaire.CurrentGroup.Groups[0].Questions.Count(); i++)
                if (_Questionnaire.CurrentGroup.Groups[0].Questions[i].PublicKey == currentQuestion.PublicKey)
                    NextQuestion = _Questionnaire.CurrentGroup.Groups[0].Questions.Count()>i+1 ? _Questionnaire.CurrentGroup.Groups[0].Questions[i+1] : null;
         }

         private CompleteQuestionnaireViewV _Questionnaire { get; set; }

        ObservableCollection<List<NavigationItem>> navigation;
         public ObservableCollection<List<NavigationItem>> Navigation
         {
             get { return navigation; }
             set { SetValue<ObservableCollection<List<NavigationItem>>>("Navigation", ref navigation, value); }
         }

         public QuestionData QuestionData { get { return (QuestionData)Data; } }

         private CompleteAnswerView selectedAnswer;
         public CompleteAnswerView SelectedAnswer
         {
             get { return selectedAnswer; }
             set { SetValue<CompleteAnswerView>("SelectedAnswer", ref selectedAnswer, value); }
         }

        private CompleteQuestionView nextQuestion;
        public CompleteQuestionView NextQuestion
        {
            get { return nextQuestion; }
            set { SetValue<CompleteQuestionView>("NextQuestion", ref nextQuestion, value); }
        }

         #region Commands
         protected override void InitializeCommands()
         {
             base.InitializeCommands();
             SetCurrentAnswerCommand = new SimpleActionCommand(DoSetCurrentAnswer);
             CloseWindowCommand = new SimpleActionCommand(DoClose);
         }

        private void DoClose(object p)
        {
            ////var singleOrDefault = Application.Current.Windows.Cast<Window>().SingleOrDefault(x => x.IsActive);
            ////if (singleOrDefault != null)
            ////    singleOrDefault.Close();
            //////window.Close;
            var next = p as CompleteQuestionView;
            if(next != null)
                InitData(next);
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
                         if (completeAnswerView.PublicKey == answer.PublicKey)
                             if (QuestionData.Question.QuestionType == QuestionType.MultyOption)
                                completeAnswerView.Selected = !completeAnswerView.Selected;
                            else
                                completeAnswerView.Selected = completeAnswerView.PublicKey == answer.PublicKey;
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
            var viewRepository = new ViewRepository(Initializer.Kernel);
            var test =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(QuestionData.Question.QuestionnaireId) { CurrentGroupPublicKey = QuestionData.Question.GroupPublicKey });
            var item = new CompleteQuestionView();
            for (int i = 0; i < test.CurrentGroup.Groups[0].Questions.Count(); i++)
                if (test.CurrentGroup.Groups[0].Questions[i].PublicKey == QuestionData.Question.PublicKey)
                    item = test.CurrentGroup.Groups[0].Questions[i];
            SetSelectedAnswer(item);
        }

        private void SetSelectedAnswer(CompleteQuestionView questionView)
        {
            if (questionView != null)
            {
                Data = new QuestionData(questionView);
                foreach (var completeAnswerView in QuestionData.Question.Answers)
                    if (completeAnswerView.Selected || QuestionData.Question.Answers.Count()==1)
                        SelectedAnswer = completeAnswerView;
            }
        }

        private void BuildMenu()
        {
            //bad -load from currentdata
            var detail = new QuestionnaireDetail();
            detail.InitData(QuestionData.Question.QuestionnaireId);
            Navigation = detail.Navigation;
        }

        #endregion

    }
}
