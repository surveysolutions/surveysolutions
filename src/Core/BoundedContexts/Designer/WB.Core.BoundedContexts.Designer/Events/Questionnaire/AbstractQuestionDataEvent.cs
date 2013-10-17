using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractQuestionDataEvent : QuestionnaireActiveEvent
    {
        public string ConditionExpression { get; set; }
        public bool Featured { get; set; }
        public string Instructions { get; set; }
        public bool Mandatory { get; set; }
        public bool Capital { get; set; }
        public Guid PublicKey { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public QuestionScope QuestionScope { get; set; }
        public string StataExportCaption { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
    }
}
