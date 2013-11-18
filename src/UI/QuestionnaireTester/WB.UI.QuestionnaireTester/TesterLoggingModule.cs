using Ninject.Modules;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;

namespace WB.UI.QuestionnaireTester
{
    public class TesterLoggingModule : NinjectModule
    {
        public override void Load()
        {
            var logger = new FileLogger("WBCapiTester");
            this.Bind<ILogger>().ToConstant(logger);
        }
    }
}