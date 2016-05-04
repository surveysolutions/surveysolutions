namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    public class GroupUpdated : FullGroupDataEvent
    {
        public Guid GroupPublicKey { get; set; }
    }
}