using Ninject.Modules;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    public class DummyLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().ToMethod(x => new DummyLogger()).InSingletonScope();
        }
    }
}
