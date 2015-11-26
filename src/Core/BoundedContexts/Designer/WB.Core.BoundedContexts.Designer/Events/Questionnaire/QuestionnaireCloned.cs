using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class QuestionnaireCloned : ILiteEvent
    {
        public QuestionnaireDocument QuestionnaireDocument { get; set; }
        public Guid ClonedFromQuestionnaireId { get; set; }
        public long ClonedFromQuestionnaireVersion { get; set; }
    }
}
