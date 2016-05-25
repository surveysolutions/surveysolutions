using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQuestionnaireUpgradeService
    {
        QuestionnaireDocument CreateRostersVariableName(QuestionnaireDocument originalDocument);
        Dictionary<string, IGroup> GetMissingVariableNames(QuestionnaireDocument originalDocument);
    }
}
