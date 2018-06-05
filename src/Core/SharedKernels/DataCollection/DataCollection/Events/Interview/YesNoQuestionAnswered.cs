using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class YesNoQuestionAnswered : QuestionAnswered
    {
        public AnsweredYesNoOption[] AnsweredOptions { get; private set; }

        public YesNoQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector,
            DateTimeOffset originDate, AnsweredYesNoOption[] answeredOptions, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.AnsweredOptions = answeredOptions;
        }
    }
}
