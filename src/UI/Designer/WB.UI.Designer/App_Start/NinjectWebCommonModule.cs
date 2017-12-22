using System;
using System.Web;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Web.Common;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Modules;
using WB.UI.Shared.Web.Settings;


namespace WB.UI.Designer.App_Start
{
    public class NinjectWebCommonModule : IWebModule
    {
        public void Load(IWebIocRegistry registry)
        {
            registry.BindAsSingleton<IAggregateRootCacheCleaner, DummyAggregateRootCacheCleaner>();

            registry.BindHttpFilterWhenControllerHasAttribute<TokenValidationAuthorizationFilter, ApiValidationAntiForgeryTokenAttribute>(
                FilterScope.Controller,
                new ConstructorArgument("tokenVerifier", _ => new ApiValidationAntiForgeryTokenVerifier()));

            registry.BindAsSingleton<ISettingsProvider, DesignerSettingsProvider>();
            registry.BindToMethod<Func<IKernel>>(ctx => () => new Bootstrapper().Kernel);
            registry.Bind<IHttpModule, HttpApplicationInitializationHttpModule>();

            registry.Bind<IAuthenticationService, AuthenticationService>();
            registry.Bind<IRecaptchaService, RecaptchaService>();
            registry.Bind<QuestionnaireDowngradeService>();
            registry.Bind<IQuestionnireHistoryVersionsService, QuestionnireHistoryVersionsService>();
        }
    }
}