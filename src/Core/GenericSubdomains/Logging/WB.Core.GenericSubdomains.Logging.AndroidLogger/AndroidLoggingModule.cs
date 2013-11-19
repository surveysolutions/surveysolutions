namespace WB.Core.GenericSubdomains.Logging.AndroidLogger
{
    using Ninject.Modules;

    public class AndroidLoggingModule : NinjectModule
    {
        public override void Load()
        {
            var logger = new FileLogger("WBCapi");
            this.Bind<ILogger>().ToConstant(logger);
        }
    }
}