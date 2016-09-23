using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class QuestionnaireUpdated : QuestionnaireActive
    {
        [Obsolete]
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}