using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using reCAPTCHA.AspNetCore;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Revisions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Modules
{
    public class WebCommonModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<IAuthenticationService, AuthenticationService>();
            registry.Bind<IRecaptchaService, RecaptchaService>();
            registry.Bind<IQuestionnaireHistoryVersionsService, QuestionnaireHistoryVersionsService>();
            registry.Bind<IVideoConverter, VideoConverter>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
