using System;

namespace Main.Core.Events.Questionnaire
{
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:NewGroupAdded")]
    public class NewGroupAdded : FullGroupDataEvent
    {
        public Guid PublicKey { get; set; }
        public Propagate Paropagateble { get; set; }

        public Guid? ParentGroupPublicKey { get; set; }
    }
}