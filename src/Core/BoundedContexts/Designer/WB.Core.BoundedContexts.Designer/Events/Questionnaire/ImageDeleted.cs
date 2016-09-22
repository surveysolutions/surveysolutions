using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class ImageDeleted: QuestionnaireActiveEvent
    {
        public Guid ImageKey { get; set; }

        public Guid QuestionKey { get; set; }
    }
}