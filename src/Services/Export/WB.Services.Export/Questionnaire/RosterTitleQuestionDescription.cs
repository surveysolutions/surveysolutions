using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class RosterTitleQuestionDescription
    {
        public RosterTitleQuestionDescription(Guid questionId, Dictionary<string, string> options = null)
        {
            this.QuestionId = questionId;
            this.Options = options ?? new Dictionary<string, string>();
        }

        public Guid QuestionId { get; private set; }
        public Dictionary<string, string> Options { get; private set; }
    }
}
