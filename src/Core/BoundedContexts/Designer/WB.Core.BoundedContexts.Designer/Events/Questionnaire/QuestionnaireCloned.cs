using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class QuestionnaireCloned : IEvent
    {
        public QuestionnaireDocument QuestionnaireDocument { get; set; }
        public Guid ClonedFromQuestionnaireId { get; set; }
        public long ClonedFromQuestionnaireVersion { get; set; }
    }
}
