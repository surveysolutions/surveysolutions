using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionAdded : AbstractNumericQuestionDataEvent
    {
        public Guid GroupPublicKey { get; set; }
    }
}
