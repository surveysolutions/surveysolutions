using System;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class QuestionnaireCloned
    {
        public QuestionnaireDocument QuestionnaireDocument { get; set; }
        public Guid ClonedFromQuestionnaireId { get; set; }
        public long ClonedFromQuestionnaireVersion { get; set; }
    }
}
