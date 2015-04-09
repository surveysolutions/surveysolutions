using System;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Model;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxViewModel
    {
        public TextQuestionViewModel(Guid questionId, InterviewModel interviewModel, QuestionnaireDocument questionnaireDocument)
        {
            TextQuestion textQuestion = questionnaireDocument.Find<TextQuestion>(questionId);

            Title = textQuestion.QuestionText;
            Answer = interviewModel.GetAnswerOnQuestion<string>(questionId);
        }

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private string answer;

        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }

    }
}