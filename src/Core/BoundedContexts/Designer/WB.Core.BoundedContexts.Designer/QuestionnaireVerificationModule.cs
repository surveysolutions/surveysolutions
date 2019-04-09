using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Designer
{
    [ExcludeFromCodeCoverage]
    public class QuestionnaireVerificationModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IQuestionnaireVerifier, QuestionnaireVerifier>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
