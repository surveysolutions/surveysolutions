namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:GroupCloned")]
    public class GroupCloned : FullGroupDataEvent
    {
        public Guid SourceGroupId { get; set; }

        public int TargetIndex { get; set; }

    }
}