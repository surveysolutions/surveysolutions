using System.IO;
using NLog;
using NLog.Config;
using Ninject.Modules;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    public class NLogLoggingModule : NinjectModule
    {
        private readonly string fileDirectory;

        public NLogLoggingModule(string fileDirectory)
        {
            this.fileDirectory = fileDirectory;
        }

        public override void Load()
        {
#if DEBUG
            var filePath = Path.Combine(fileDirectory, "NLog.Debug.config");
            if (File.Exists(filePath))
                LogManager.Configuration = new XmlLoggingConfiguration(filePath);
#endif
            this.Bind<ILogger>().ToConstant(new NLogLogger());
        }
    }
}
