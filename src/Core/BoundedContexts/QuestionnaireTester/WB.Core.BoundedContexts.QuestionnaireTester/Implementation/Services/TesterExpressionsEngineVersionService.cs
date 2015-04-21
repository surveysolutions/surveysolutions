using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class TesterExpressionsEngineVersionService : ITesterExpressionsEngineVersionService
    {
        private readonly QuestionnaireVersion questionnaireVersion = new QuestionnaireVersion
        {
            Major = 6,
            Minor = 0,
            Patch = 0
        };

        public QuestionnaireVersion GetExpressionsEngineSupportedVersion()
        {
            return this.questionnaireVersion;
        }
    }
}
