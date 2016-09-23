using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class GroupCloned : FullGroupData
    {
        public Guid PublicKey { get; set; }
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceGroupId { get; set; }
        public int TargetIndex { get; set; }
        public Guid? ParentGroupPublicKey { get; set; }
    }
}