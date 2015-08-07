using Cirrious.MvvmCross.Plugins.Location;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.Android.Implementation.Services.FileSystem;
using WB.Core.Infrastructure.Android.Implementation.Services.Security;
using WB.Core.Infrastructure.Android.Implementation.Services.Settings;
using WB.Core.Infrastructure.Android.Implementation.Services.Utility;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Tester.CustomServices.Location;

namespace WB.Infrastructure.Shared.Enumerator.Ninject
{
    public class EnumeratorInfrastructureModule : NinjectModule
    {
        private const string dataDirectoryName = "InterviewData";

        private readonly string basePath;

        public EnumeratorInfrastructureModule(string basePath)
        {
            this.basePath = basePath;
        }

        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();
            this.Bind<IUserIdentity>().ToMethod(context => context.Kernel.Get<IPrincipal>().CurrentUserIdentity);

            this.Bind<IExpressionsEngineVersionService>().To<ExpressionsEngineVersionService>().InSingletonScope();
            this.Bind<ISettingsProvider>().To<ApplicationSettings>();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();

            this.Bind<IPlainInterviewFileStorage>().To<PlainInterviewFileStorage>().InSingletonScope()
                .WithConstructorArgument("rootDirectoryPath", this.basePath)
                .WithConstructorArgument("dataDirectoryName", dataDirectoryName);

            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepository>().InSingletonScope();
            this.Bind<IQuestionnaireRepository>().ToMethod<IQuestionnaireRepository>(context => context.Kernel.Get<IPlainQuestionnaireRepository>());

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnaireAssemblyFileAccessor>().InSingletonScope()
                .WithConstructorArgument("assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies"));

            this.Bind<IQrBarcodeScanService>().To<QrBarcodeScanService>();

            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IMvxLocationWatcher>().To<PlayServicesLocationWatcher>();
        }
    }
}