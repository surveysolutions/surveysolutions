using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class ImageUpdated
    {
        public string Description { get; set; }

        public Guid ImageKey { get; set; }

        public Guid QuestionKey { get; set; }

        public string Title { get; set; }
    }
}