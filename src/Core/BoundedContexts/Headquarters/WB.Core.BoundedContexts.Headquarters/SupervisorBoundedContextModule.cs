using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Implementation.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.ViewFactories;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISurveyViewFactory>().To<SurveyViewFactory>();
        }
    }
}
