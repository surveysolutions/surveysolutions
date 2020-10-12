using System;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class OptionBorderViewModel : MvxNotifyPropertyChanged, ICompositeEntity
    {
        private bool hasOptions;

        public OptionBorderViewModel(IQuestionStateViewModel questionState,
            bool isTop)
        {
            this.QuestionState = questionState ?? throw new ArgumentNullException(nameof(questionState));

            this.IsTop = isTop;
            this.hasOptions = true;
        }

        public bool HasOptions
        {
            get => this.hasOptions;
            set => this.SetProperty(ref this.hasOptions, value);
        }

        public IQuestionStateViewModel QuestionState { get; set; }

        public bool IsTop { get; set; }
    }
}