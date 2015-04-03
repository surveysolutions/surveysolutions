using Ninject.Modules;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class CommonModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ApplicationSettings>().ToSelf().InSingletonScope();
        }
    }
}