using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsQuestionAnswered : QuestionAnswered
    {
        public Guid[] SelectedOptions { get; private set; }

        public MultipleOptionsQuestionAnswered(Guid userId, Guid questionId, DateTime answerTime, Guid[] selectedOptions)
            : base(userId, questionId, answerTime)
        {
            this.SelectedOptions = selectedOptions;
        }
    }
}