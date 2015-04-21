using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface ITesterExpressionsEngineVersionService
    {
        QuestionnaireVersion GetExpressionsEngineSupportedVersion();
    }
}
