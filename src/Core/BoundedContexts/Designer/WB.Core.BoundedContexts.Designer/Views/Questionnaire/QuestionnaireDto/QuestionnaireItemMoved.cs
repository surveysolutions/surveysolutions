using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class QuestionnaireItemMoved : QuestionnaireActive
    {

        public Guid? GroupKey { get; set; }
        public Guid PublicKey { get; set; }

        [Obsolete]
        public Guid QuestionnaireId { get; set; }

        public int TargetIndex { get; set; }
    }
}