using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractNumericQuestionDataEvent : AbstractQuestionDataEvent
    {
        public List<Guid> Triggers { get; set; }
        public bool IsAutopropagating { get; set; }

        [Obsolete("Property is obsolete, actual only for old AutoPropagate question, had default value 10.")]
        public int? MaxValue { get; private set; }

        public int? MaxAllowedValue { get; set; }

        public bool? IsInteger { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
    }
}
