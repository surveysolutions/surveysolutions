using Cirrious.MvvmCross.Plugins.Location;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.BoundedContexts.Tester;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Infrastructure.Shared.Enumerator.Internals;
using WB.Infrastructure.Shared.Enumerator.Internals.FileSystem;
using WB.Infrastructure.Shared.Enumerator.Internals.Location;
using WB.Infrastructure.Shared.Enumerator.Internals.Security;
using WB.Infrastructure.Shared.Enumerator.Internals.Settings;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class EnumeratorInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();
            this.Bind<IUserIdentity>().ToMethod(context => context.Kernel.Get<IPrincipal>().CurrentUserIdentity);

            this.Bind<IExpressionsEngineVersionService>().To<ExpressionsEngineVersionService>().InSingletonScope();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnaireAssemblyFileAccessor>().InSingletonScope()
                .WithConstructorArgument("assemblyStorageDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("assemblies"));

            this.Bind<IQRBarcodeScanService>().To<QRBarcodeScanService>();

            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IMvxLocationWatcher>().To<PlayServicesLocationWatcher>();
        }
    }
}