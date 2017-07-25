using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Utils;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Internals.FileSystem;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : NinjectModule
    {
        public override void Load()
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            this.Bind<IUserInteractionService>().To<UserInteractionService>();
            this.Bind<IUserInterfaceStateService>().To<UserInterfaceStateService>();

            this.Bind<IExternalAppLauncher>().To<ExternalAppLauncher>();
            this.Bind<IPermissionsService>().To<PermissionsService>();
            this.Bind<IVirbationService>().To<VibrationService>();
            this.Bind<IAudioService>().To<AudioService>().InSingletonScope().WithConstructorArgument("pathToAudioDirectory",
                AndroidPathUtils.GetPathToSubfolderInLocalDirectory("audio"));
            this.Bind<IAudioDialog>().To<AudioDialog>().InSingletonScope();

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocatorAdapter(this.Kernel));
            this.Bind<IServiceLocator>().ToConstant(ServiceLocator.Current);

            this.Bind<IEventTypeResolver>().ToConstant(
                new EventTypeResolver(
                    typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                    typeof(EnumeratorSharedKernelModule).Assembly));

            this.Bind<IEnumeratorArchiveUtils>().To<ZipArchiveUtils>();

            this.Bind<IFileSystemAccessor>().To<FileSystemService>().InSingletonScope();
            this.Bind<IQRBarcodeScanService>().To<QRBarcodeScanService>();
            this.Bind<IPictureChooser>().To<PictureChooser>();
            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IGeolocator>().ToMethod(context => CrossGeolocator.Current);
            this.Bind<IMedia>().ToMethod(context => CrossMedia.Current);
            this.Bind<IPermissions>().ToMethod(context => CrossPermissions.Current);

            this.Bind<IAttachmentContentStorage>().To<AttachmentContentStorage>().InSingletonScope();
            this.Bind<ITranslationStorage>().To<TranslationsStorage>();
            this.Bind<IPasswordHasher>().To<DevicePasswordHasher>().InSingletonScope();
            this.Bind<IHttpStatistician>().To<HttpStatistician>().InSingletonScope();
        }
    }
}