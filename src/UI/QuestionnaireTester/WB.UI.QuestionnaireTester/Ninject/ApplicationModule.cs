using Ninject.Modules;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class ApplicationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
        }
    }
}