using Ninject.Modules;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    public class NLogLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().ToMethod(x => new NLogLogger("")).InSingletonScope();
        }
    }
}
