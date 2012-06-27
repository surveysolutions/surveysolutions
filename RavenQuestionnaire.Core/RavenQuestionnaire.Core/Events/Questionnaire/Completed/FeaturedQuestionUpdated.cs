using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:FeaturedQuestionUpdated")]
    public class FeaturedQuestionUpdated
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public Guid QuestionPublicKey { set; get; }
        public string QuestionText { get; set; }
        public object Answer { set; get; }
    }
}
