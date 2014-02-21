using Ninject.Modules;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.CapiInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.InfoFileSupplierRegistry;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;

namespace WB.Core.GenericSubdomains.ErrorReporting
{
    public class ErrorReportingModule : NinjectModule
    {
        private readonly string basePath;

        public ErrorReportingModule(string basePath)
        {
            this.basePath = basePath;
        }

        public override void Load()
        {
            this.Bind<IInfoFileSupplierRegistry>().To<DefaultInfoFileSupplierRegistry>().InSingletonScope();
            this.Bind<ITabletInformationSenderFactory>().To<TabletInformationSenderFactory>();
        
            this.Bind<ICapiInformationService>()
                .To<CapiInformationService>()
                .InSingletonScope().WithConstructorArgument("basePath", this.basePath);
        }
    }
}
