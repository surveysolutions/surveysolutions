using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Services
{
    public interface IQuestionnaireUpgradeService
    {
        QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument);
        Dictionary<string, IGroup> GetMissingVariableNames(QuestionnaireDocument originalDocument);
    }
}
