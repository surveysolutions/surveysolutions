using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.QuestionnaireUpgrader.Services
{
    public interface IQuestionnaireUpgradeService
    {
        QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument);
    }
}
