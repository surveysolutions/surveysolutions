using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class GroupCloned : FullGroupDataEvent
    {
        public Guid PublicKey { get; set; }
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceGroupId { get; set; }
        public int TargetIndex { get; set; }
        public Guid? ParentGroupPublicKey { get; set; }
    }
}