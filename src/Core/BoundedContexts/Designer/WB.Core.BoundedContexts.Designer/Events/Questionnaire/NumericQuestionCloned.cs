using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionCloned : AbstractNumericQuestionDataEvent
    {
        public Guid SourceQuestionId { get; set; }
        public Guid GroupPublicKey { get; set; }
        public int TargetIndex { get; set; }
    }
}
