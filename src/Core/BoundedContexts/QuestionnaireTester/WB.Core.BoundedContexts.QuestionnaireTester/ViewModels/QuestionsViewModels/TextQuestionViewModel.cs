using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxViewModel
    {
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