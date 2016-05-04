using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Tester
{
    public class TesterBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();
        }
    }
}