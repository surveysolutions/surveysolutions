using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Logging
{
    public class NLogLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().ToMethod(context =>
            {
                var typeForLogger = context.Request.Target != null
                                    ? context.Request.Target.Member.DeclaringType
                                    : context.Request.Service;
                return new NLogLogger(typeForLogger);
            });
        }
    }
}
