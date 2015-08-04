using Ninject.Modules;

using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Native.Logging
{
    public class NLogLoggingModule : NinjectModule
    {
        public NLogLoggingModule(){}

        public override void Load()
        {
            this.Bind<ILogger>().To<NLogLogger>();
        }
    }
}
