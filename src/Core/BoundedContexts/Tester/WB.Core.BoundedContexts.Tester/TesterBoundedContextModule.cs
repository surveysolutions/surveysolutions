using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Tester
{
    public class TesterBoundedContextModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IQuestionnaireImportService, QuestionnaireImportService>();

            registry.Bind<LoginViewModel>();
            registry.Bind<DashboardViewModel>();
            registry.Bind<QuestionnaireDownloadViewModel>();
            registry.Bind<PrefilledQuestionsViewModel>();
            registry.Bind<InterviewViewModel>();
        }
    }
}