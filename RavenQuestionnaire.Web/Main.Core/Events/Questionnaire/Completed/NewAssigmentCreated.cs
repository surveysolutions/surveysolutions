using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace Main.Core.Events.Questionnaire.Completed
{
    public class NewAssigmentCreated
    {
        public CompleteQuestionnaireDocument Source { get; set; }
    }
}
