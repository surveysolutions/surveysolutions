using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Security;

namespace WB.Core.Infrastructure.Android
{
    public class SecurityModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();
        }
    }
}