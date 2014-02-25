using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemInputModel
    {
        public QuestionnaireItemInputModel(Guid id)
        {
            this.QuestionnaireId = id;
        }
       
        public Guid QuestionnaireId { get; private set; }
    }
}
