using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class RosterTitleQuestionDescription
    {
        public RosterTitleQuestionDescription(Guid questionId, Dictionary<int, string> options = null)
        {
            this.QuestionId = questionId;
            this.Options = options ?? new Dictionary<int, string>();
        }

        public Guid QuestionId { get; private set; }
        public Dictionary<int, string> Options { get; private set; }
    }
}
