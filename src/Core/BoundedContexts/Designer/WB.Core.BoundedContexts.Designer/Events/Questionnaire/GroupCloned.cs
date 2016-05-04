namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    public class GroupCloned : FullGroupDataEvent
    {
        public Guid PublicKey { get; set; }
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceGroupId { get; set; }
        public int TargetIndex { get; set; }
        public Guid? ParentGroupPublicKey { get; set; }
    }
}