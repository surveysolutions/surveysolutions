using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class OptionBorderViewModel<T> : ICompositeEntity where T : QuestionAnswered
    {
        public OptionBorderViewModel(QuestionStateViewModel<T> questionState,
            bool isTop)
        {
            if (questionState == null) throw new ArgumentNullException(nameof(questionState));

            this.QuestionState = questionState;
            this.IsTop = isTop;
        }

        public QuestionStateViewModel<T> QuestionState { get; set; }

        public bool IsTop { get; set; }
    }
}