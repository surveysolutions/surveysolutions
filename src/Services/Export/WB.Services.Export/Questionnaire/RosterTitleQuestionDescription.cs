using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class RosterTitleQuestionDescription
    {
        public RosterTitleQuestionDescription(Guid questionId, Dictionary<decimal, string> options = null)
        {
            this.QuestionId = questionId;
            this.Options = options ?? new Dictionary<decimal, string>();
        }

        public Guid QuestionId { get; private set; }
        public Dictionary<decimal, string> Options { get; private set; }
    }
}