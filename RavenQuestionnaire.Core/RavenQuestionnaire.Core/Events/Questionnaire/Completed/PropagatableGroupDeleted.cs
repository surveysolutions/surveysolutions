using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:PropagatableGroupDeleted")]
    public class PropagatableGroupDeleted
    {
        public Guid CompletedQuestionnaireId { get; set; }

        public Guid PublicKey { get; set; }
        public Guid PropagationKey { get; set; }

    }
}
