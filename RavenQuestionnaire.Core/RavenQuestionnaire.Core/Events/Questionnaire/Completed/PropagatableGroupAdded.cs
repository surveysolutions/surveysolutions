using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:PropagatableGroupAdded")]
    public class PropagatableGroupAdded
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public ICompleteGroup Group { get; set; }

    }
}
