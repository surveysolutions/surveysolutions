using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class NewGroupAdded : FullGroupData
    {
        public Guid PublicKey { get; set; }

        public Guid? ParentGroupPublicKey { get; set; }
    }
}