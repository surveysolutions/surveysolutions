using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemInputModel
    {
        public QuestionnaireItemInputModel(Guid id, long version)
        {
            this.QuestionnaireId = id;
            this.Version = version;
        }
       
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }
    }
}
