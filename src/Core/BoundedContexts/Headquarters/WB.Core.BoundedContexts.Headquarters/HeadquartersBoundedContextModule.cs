using System;
using System.Web;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        public override void VerifyRequiredModulesAreLoaded()
        {
            if (!this.Kernel.HasModule(typeof(PasswordPolicyModule).FullName))
            {
                throw new InvalidOperationException("PasswordPolicyModule is required");
            }
        }

        public override void Load()
        {
            this.Bind<ISurveyViewFactory>().To<SurveyViewFactory>();

            this.Bind<IEventHandler>().To<SurveyLineViewDenormalizer>();
            this.Bind<IEventHandler>().To<SurveyDetailsViewDenormalizer>();

            DispatcherRegistryHelper.RegisterDenormalizer<SupervisorLoginsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<SupervisorCredentialsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<SupervisorFeedDenormalizer>(this.Kernel);

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable
            this.Bind<ISupervisorLoginService>().To<SupervisorLoginService>().InSingletonScope();
            this.Bind<ISupervisorFeedService>().To<SupervisorFeedService>();

            this.Bind<IDesignerService>().To<DesignerService>();

            this.Unbind(typeof(HttpContextBase));
            this.Bind<HttpContextBase>().ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InTransientScope();
        }
    }
}
