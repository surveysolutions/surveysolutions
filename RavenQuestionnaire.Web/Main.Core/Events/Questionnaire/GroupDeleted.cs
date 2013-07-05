namespace Main.Core.Events.Questionnaire
{
    using System;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:GroupDeleted")]
    public class GroupDeleted
    {
        /// <remarks>Needed for successfull deserialization from DB</remarks>>
        public GroupDeleted() {}

        public GroupDeleted(Guid groupPublicKey)
        {
            this.GroupPublicKey = groupPublicKey;
        }

        [Obsolete]
        public GroupDeleted(Guid groupPublicKey, Guid parentPublicKey)
            : this(groupPublicKey)
        {
            this.ParentPublicKey = parentPublicKey;
        }

        public Guid GroupPublicKey { get; set; }

        [Obsolete]
        public Guid ParentPublicKey { get; set; }
    }
}