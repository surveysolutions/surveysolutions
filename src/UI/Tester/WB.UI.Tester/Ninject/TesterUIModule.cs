using Ninject.Modules;

using WB.Core.BoundedContexts.Tester.Services;
using WB.UI.Tester.CustomServices.UserInteraction;

namespace WB.UI.Tester.Ninject
{
    public class TesterUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IUserInteractionService>().To<UserInteractionService>();
        }
    }
}