using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IDesignerEngineVersionService
    {
        int LatestSupportedVersion { get; }

        int GetQuestionnaireContentVersion(QuestionnaireDocument questionnaireDocument);

        bool IsClientVersionSupported(int clientVersion);

        IEnumerable<string> GetListOfNewFeaturesForClient(QuestionnaireDocument questionnaire, int clientVersion);

        bool DoesQuestionnaireSupportCriticality(QuestionnaireDocument questionnaire);
    }
}
