using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Events.Questionnaire
{
    public class QuestionnaireDisabled
    {
        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
