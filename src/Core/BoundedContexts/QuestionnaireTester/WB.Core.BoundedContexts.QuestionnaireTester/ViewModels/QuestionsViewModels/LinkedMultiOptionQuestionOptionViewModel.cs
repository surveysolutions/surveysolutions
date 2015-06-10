using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class LinkedMultiOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        private string title;
        private bool @checked;

        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public decimal[] Value { get; set; }

        public bool Checked
        {
            get { return this.@checked; }
            set { this.@checked = value; this.RaisePropertyChanged(); }
        }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(() => { });
            }
        }
    }
}