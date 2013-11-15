namespace WB.Core.GenericSubdomains.Logging.AndroidLogger
{
    using Ninject.Modules;

    public class AndroidLoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILogger>().To<FileLogger>();
        }
    }
}