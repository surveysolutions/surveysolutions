using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class PropagatedQuestion
    {
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public string Instructions { get; set; }

        public List<CompleteQuestionView> Questions { get; set; }

        public PropagatedQuestion()
        {

        }
    }
}