using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class TextListQuestionCloned : AbstractListQuestionData
    {
        public Guid? SourceQuestionnaireId { get; set; }
        public Guid SourceQuestionId { get; set; }
        public Guid GroupId { get; set; }
        public int TargetIndex { get; set; }
    }
}
