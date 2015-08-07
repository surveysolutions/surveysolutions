using System.Security.Policy;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using PCLStorage;
using Sqo;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.FileSystem;
using WB.Core.Infrastructure.Android.Implementation.Services.Json;
using WB.Core.Infrastructure.Android.Implementation.Services.Log;
using WB.Core.Infrastructure.Android.Implementation.Services.Network;
using WB.Core.Infrastructure.Android.Implementation.Services.Rest;
using WB.Core.Infrastructure.Android.Implementation.Services.Security;
using WB.Core.Infrastructure.Android.Implementation.Services.Settings;
using WB.Core.Infrastructure.Android.Implementation.Services.Storage;
using WB.Core.Infrastructure.Android.Implementation.Services.Utility;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;

namespace WB.Core.Infrastructure.Android
{
    public class AndroidInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            this.Bind<ILogger>().To<XamarinInsightsLogger>().InSingletonScope();

            this.Bind<IPrincipal>().To<Principal>().InSingletonScope();
            this.Bind<IUserIdentity>().ToMethod(_ => _.Kernel.Get<IPrincipal>().CurrentUserIdentity);

            this.Bind<NewtonJsonSerializer>().ToSelf().InSingletonScope();

            this.Bind<INetworkService>().To<NetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();

            this.Bind<IExpressionsEngineVersionService>().To<ExpressionsEngineVersionService>().InSingletonScope();
            this.Bind<ISettingsProvider>().To<ApplicationSettings>();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();

            this.Bind(typeof(IPlainStorageAccessor<>)).To(typeof(PlainStorageAccessor<>)).InSingletonScope();

            this.Bind<IQrBarcodeScanService>().To<QrBarcodeScanService>();
        }
    }
}