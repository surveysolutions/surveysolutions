namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:GroupUpdated")]
    public class GroupUpdated : FullGroupDataEvent
    {
        public Guid GroupPublicKey { get; set; }
        public Propagate Propagateble { get; set; }
    }
}