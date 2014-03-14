using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure.EventBus;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISurveyViewFactory>().To<SurveyViewFactory>();

            this.Bind<IEventHandler>().To<SurveyLineViewDenormalizer>();
            this.Bind<IEventHandler>().To<SurveyDetailsViewDenormalizer>();

            this.Bind<IPasswordHasher>().To<DummyPasswordHasher>().InSingletonScope();
        }
    }
}
