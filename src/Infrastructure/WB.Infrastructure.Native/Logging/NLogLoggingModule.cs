using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Logging
{
    public class NLogLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILoggerProvider>().To<NLogLoggerProvider>();

            this.Bind<ILogger>().ToMethod(context =>
            {
                if (context.Request.Target != null)
                    return new NLogLogger(context.Request.Target.Member.DeclaringType);

                return new NLogLogger("UNKNOWN");
            });
        }
    }
}
