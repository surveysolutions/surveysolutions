using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:Delete")]
    public class CompleteQuestionnaireDeleted
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public Guid TemplateId { get; set; }
    }
}
