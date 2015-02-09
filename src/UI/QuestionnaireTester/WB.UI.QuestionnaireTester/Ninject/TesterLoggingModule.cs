using Ninject.Modules;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Ninject
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