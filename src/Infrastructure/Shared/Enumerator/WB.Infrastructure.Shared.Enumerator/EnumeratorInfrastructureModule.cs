using Flurl.Http;
using Geolocator.Plugin;
using Geolocator.Plugin.Abstractions;
using ICSharpCode.SharpZipLib;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
            this.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            this.Bind<IEventTypeResolver>().ToConstant(
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(EnumeratorSharedKernelModule).Assembly));

            VFS.SetCurrent(new AndroidFileSystem());
            this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();
            this.Bind<IQRBarcodeScanService>().To<QRBarcodeScanService>();
            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IGeolocator>().ToMethod(context => CrossGeolocator.Current);

            FlurlHttp.Configure(c => {
                c.HttpClientFactory = new ModernHttpClientFactory();
            });

            this.Bind<IAttachmentContentStorage>().To<AttachmentContentStorage>().InSingletonScope();
            this.Bind<ITranslationStorage>().To<TranslationsStorage>();
        }
    }
}