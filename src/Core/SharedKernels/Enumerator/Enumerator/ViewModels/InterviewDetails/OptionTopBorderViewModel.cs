using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class OptionBorderViewModel<T> : 
        MvxNotifyPropertyChanged,
        ICompositeEntity where T : QuestionAnswered
    {
        private bool hasOptions;

        public OptionBorderViewModel(QuestionStateViewModel<T> questionState,
            bool isTop)
        {
            if (questionState == null) throw new ArgumentNullException(nameof(questionState));

            this.QuestionState = questionState;
            this.IsTop = isTop;
            this.hasOptions = true;
        }

        public bool HasOptions
        {
            get { return this.hasOptions; }
            set
            {
                this.hasOptions = value; 
                this.RaisePropertyChanged();
            }
        }

        public QuestionStateViewModel<T> QuestionState { get; set; }

        public bool IsTop { get; set; }
    }
}