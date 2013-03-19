namespace Main.Core.Events.Questionnaire
{
    using System;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewGroupAdded")]
    public class NewGroupAdded
    {
        public string ConditionExpression { get; set; }

        public string GroupText { get; set; }

        public Guid? ParentGroupPublicKey { get; set; }

        public Propagate Paropagateble { get; set; }

        public Guid PublicKey { get; set; }

        [Obsolete]
        public Guid QuestionnairePublicKey { get; set; }

        public string Description { get; set; }
    }
}