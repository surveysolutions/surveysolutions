using System;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxViewModel
    {
        public TextQuestionViewModel()
        {
        }    
        
        public TextQuestionViewModel(QuestionnaireDocument document, Guid questionId)
        {
            TextQuestion textQuestion = document.Find<TextQuestion>(questionId);

            Title = textQuestion.QuestionText;
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
//
//        public ICommand TextChanged
//        {
//            get
//            {
//                return new MvxCommand(() => ());
//            }
//        }
    }
}