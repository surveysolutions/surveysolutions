using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Tester
{
    public class TesterBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<ITesterExpressionsEngineVersionService, TesterExpressionsEngineVersionService>();
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();
            registry.BindAsSingleton<IInterviewCompletionService, TesterInterviewCompletionService>();
        }
    }
}