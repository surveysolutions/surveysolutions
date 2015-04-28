using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Settings;

namespace WB.Core.Infrastructure.Android
{
    public class SettingsModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IExpressionsEngineVersionService>().To<ExpressionsEngineVersionService>().InSingletonScope();
            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
        }
    }
}