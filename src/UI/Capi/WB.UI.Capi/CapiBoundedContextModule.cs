using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Services;
using WB.UI.Capi.Syncronization;

namespace WB.UI.Shared.Android
{
    public class CapiBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICapiCleanUpService>().To<CapiCleanUpService>();
        }
    }
}
