using System;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MultiOptionQuestionOptionViewModel : MvxNotifyPropertyChanged
    {
        private readonly MultiOptionQuestionViewModel questionViewModel;

        public MultiOptionQuestionOptionViewModel(MultiOptionQuestionViewModel questionViewModel)
        {
            this.questionViewModel = questionViewModel;
        }

        public decimal Value { get; set; }
        public string Title { get; set; }
        
        private bool @checked;
        public bool Checked
        {
            get { return @checked; }
            set
            {
                @checked = value;
                RaisePropertyChanged();
            }
        }

        public IMvxCommand CheckAnswerCommand
        {
            get
            {
                return new MvxCommand(() => questionViewModel.ToggleAnswer(this));
            }
        }
    }
}