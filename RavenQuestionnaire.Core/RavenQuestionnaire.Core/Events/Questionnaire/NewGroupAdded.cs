using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewGroupAdded")]
    public class NewGroupAdded
    {
        public Guid PublicKey { set; get; }
        
        public string GroupText { get;set;}
        public Propagate Paropagateble {get; set;}
        public Guid? ParentGroupPublicKey {get; set;}
        public string ConditionExpression { get; set; }
    }
}
