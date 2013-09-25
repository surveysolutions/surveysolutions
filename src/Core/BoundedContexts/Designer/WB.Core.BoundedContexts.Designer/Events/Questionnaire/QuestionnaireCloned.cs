using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
