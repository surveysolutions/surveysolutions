using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Log;

namespace WB.Core.Infrastructure.Android
{
    public class LoggerModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();
        }
    }
}