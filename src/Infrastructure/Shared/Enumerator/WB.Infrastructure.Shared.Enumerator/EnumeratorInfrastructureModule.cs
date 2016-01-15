using Geolocator.Plugin;
using Geolocator.Plugin.Abstractions;
using ICSharpCode.SharpZipLib;
using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Infrastructure.Shared.Enumerator.Internals;
using WB.Infrastructure.Shared.Enumerator.Internals.FileSystem;

namespace WB.Infrastructure.Shared.Enumerator
{
    public class EnumeratorInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
            this.Kernel.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            VFS.SetCurrent(new AndroidFileSystem());
            this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();
            this.Bind<IAsynchronousFileSystemAccessor>().To<AsynchronousFileSystemAccessor>().InSingletonScope();
            this.Bind<IQRBarcodeScanService>().To<QRBarcodeScanService>();
            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IGeolocator>().ToMethod(context => CrossGeolocator.Current);
        }
    }
}