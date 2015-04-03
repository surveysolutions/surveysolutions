using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class SecurityModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();
        }
    }
}