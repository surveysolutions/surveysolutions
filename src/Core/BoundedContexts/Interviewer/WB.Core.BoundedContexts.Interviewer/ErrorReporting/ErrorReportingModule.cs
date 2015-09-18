using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Implementation.CapiInformation;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Implementation.InfoFileSupplierRegistry;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.CapiInformationService;
using WB.Core.BoundedContexts.Interviewer.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Interviewer.ErrorReporting
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
        
            this.Bind<ICapiInformationService>()
                .To<CapiInformationService>()
                .InSingletonScope().WithConstructorArgument("basePath", this.pathToTemporaryFolder);
        }
    }
}
