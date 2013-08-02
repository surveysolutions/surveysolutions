using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionQuestionAnswered : QuestionAnswered
    {
        public Guid SelectedOption { get; private set; }

        public SingleOptionQuestionAnswered(Guid userId, Guid questionId, DateTime answerTime, Guid selectedOption)
            : base(userId, questionId, answerTime)
        {
            this.SelectedOption = selectedOption;
        }
    }
}