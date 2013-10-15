using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionCloned : AbstractQuestionDataEvent
    {
        public Guid SourceQuestionId { get; set; }
        public Guid GroupPublicKey { get; set; }
        public int TargetIndex { get; set; }

        public List<Guid> Triggers { get; set; }
        public int MaxValue { get; set; }
        public bool? IsInteger { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
    }
}
