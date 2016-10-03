using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class ImageDeleted: QuestionnaireActive
    {
        public Guid ImageKey { get; set; }

        public Guid QuestionKey { get; set; }
    }
}