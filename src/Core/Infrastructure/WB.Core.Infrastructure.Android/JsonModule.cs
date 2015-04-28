using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Json;

namespace WB.Core.Infrastructure.Android
{
    public class JsonModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>().InSingletonScope();
        }
    }
}