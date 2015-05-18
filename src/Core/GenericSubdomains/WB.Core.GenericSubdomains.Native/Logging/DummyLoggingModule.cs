using Ninject.Modules;

using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Native.Logging
{
    public class DummyLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().ToMethod(x => new DummyLogger()).InSingletonScope();
        }
    }
}
