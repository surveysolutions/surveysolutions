using System;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:GroupUpdated")]
    public class GroupUpdated
    {
        public string GroupText { get; set; }
        public Propagate Propagateble { get; set; }
        public Guid GroupPublicKey { get; set; }
        //public List<Guid> Triggers { get; set; }
        public string QuestionnaireId { get; set; }
        public UserLight Executor { get; set; }
        public string ConditionExpression { get; set; }

    }
}
