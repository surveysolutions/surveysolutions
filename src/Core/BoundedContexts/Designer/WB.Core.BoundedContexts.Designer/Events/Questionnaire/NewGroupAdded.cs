namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:NewGroupAdded")]
    public class NewGroupAdded : FullGroupDataEvent
    {
        [Obsolete]
        public Guid QuestionnairePublicKey { get; set; }
    }
}