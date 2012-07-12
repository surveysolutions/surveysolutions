using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:GroupDeleted")]
    public class GroupDeleted
    {
        public Guid GroupPublicKey { get; set; }
    }
}
