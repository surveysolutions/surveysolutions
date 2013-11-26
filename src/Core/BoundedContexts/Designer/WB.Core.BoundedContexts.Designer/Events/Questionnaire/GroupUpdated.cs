namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:GroupUpdated")]
    public class GroupUpdated : FullGroupDataEvent
    {
        public Guid GroupPublicKey { get; set; }
    }
}