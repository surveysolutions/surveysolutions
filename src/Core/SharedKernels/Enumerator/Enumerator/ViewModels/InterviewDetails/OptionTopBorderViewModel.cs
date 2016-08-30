using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class OptionTopBorderViewModel<T> : OptionBorderViewModel<T> where T : QuestionAnswered
    {
        public OptionTopBorderViewModel(QuestionStateViewModel<T> questionState) : base(questionState)
        {
        }
    }

    public class OptionBottomBorderViewModel<T> : OptionBorderViewModel<T> where T : QuestionAnswered
    {
        public OptionBottomBorderViewModel(QuestionStateViewModel<T> questionState) : base(questionState)
        {
        }
    }

    public abstract class OptionBorderViewModel<T> where T : QuestionAnswered
    {
        public OptionBorderViewModel(QuestionStateViewModel<T> questionState)
        {
            if (questionState == null) throw new ArgumentNullException(nameof(questionState));

            this.QuestionState = questionState;
        }

        public QuestionStateViewModel<T> QuestionState { get; set; }
    }
}