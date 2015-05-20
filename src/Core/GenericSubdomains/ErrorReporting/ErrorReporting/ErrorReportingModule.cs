using Ninject.Modules;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.CapiInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.InfoFileSupplierRegistry;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.ErrorReporting
{
    public class ErrorReportingModule : NinjectModule
    {
        private readonly string pathToTemporaryFolder;

        public ErrorReportingModule(string pathToTemporaryFolder)
        {
            this.pathToTemporaryFolder = pathToTemporaryFolder;
        }

        public override void Load()
        {
            this.Bind<IInfoFileSupplierRegistry>().To<DefaultInfoFileSupplierRegistry>().InSingletonScope();
            this.Bind<ITabletInformationSender>().To<TabletInformationSender>().WithConstructorArgument("pathToTemporaryFolder", this.pathToTemporaryFolder);
        
            this.Bind<ICapiInformationService>()
                .To<CapiInformationService>()
                .InSingletonScope().WithConstructorArgument("basePath", this.pathToTemporaryFolder);
        }
    }
}
