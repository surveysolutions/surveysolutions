using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Factories
{
    public interface IQuestionnaireRosterStructureFactory
    {
        QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument questionnaire, long version);
    }
}
