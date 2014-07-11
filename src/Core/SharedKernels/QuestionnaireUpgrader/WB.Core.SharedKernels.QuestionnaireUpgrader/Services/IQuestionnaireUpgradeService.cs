using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.QuestionnaireUpgrader.Services
{
    public interface IQuestionnaireUpgradeService
    {
        QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument);
        Dictionary<string, IGroup> GetMissingVariableNames(QuestionnaireDocument originalDocument);
    }
}
