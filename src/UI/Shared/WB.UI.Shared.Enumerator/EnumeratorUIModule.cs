using System.Threading.Tasks;
using MvvmCross.ViewModels;
using Ncqrs.Eventing.Storage;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Utils;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.Shared.Enumerator.Services.Internals.FileSystem;

namespace WB.UI.Shared.Enumerator
{
    public class EnumeratorUIModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IUserInteractionService, UserInteractionService>();
            registry.Bind<IUserInterfaceStateService, UserInterfaceStateService>();
            registry.Bind<IExternalAppLauncher, ExternalAppLauncher>();
            registry.Bind<IPermissionsService, PermissionsService>();
            registry.Bind<IVirbationService, VibrationService>();
            registry.Bind<IMvxViewModelTypeFinder, MvxViewModelViewTypeFinder>();
            registry.Bind<MvxViewModelViewTypeFinder>();
            registry.Bind<IMvxViewModelByNameLookup, MvxViewModelByNameLookup>();
            registry.Bind<IMvxNameMapping, MvxViewToViewModelNameMapping>();
            
            registry.BindAsSingletonWithConstructorArgument<IAudioService, AudioService>("pathToAudioDirectory", AndroidPathUtils.GetPathToSubfolderInLocalDirectory("audio"));
            registry.BindAsSingleton<IAudioDialog, AudioDialog>();
            registry.BindToMethod<IServiceLocator>(() => ServiceLocator.Current);
            registry.BindToConstant<IEventTypeResolver>(() => new EventTypeResolver(
                typeof(DataCollectionSharedKernelAssemblyMarker).Assembly,
                typeof(EnumeratorSharedKernelModule).Assembly));
            
            registry.Bind<IEnumeratorArchiveUtils, ZipArchiveUtils>();
            registry.BindAsSingleton<IFileSystemAccessor, FileSystemService>();
            registry.Bind<IQRBarcodeScanService, QRBarcodeScanService>();
            registry.Bind<IPictureChooser, PictureChooser>();
            registry.BindAsSingleton<IGpsLocationService, GpsLocationService>();
            
            registry.BindAsSingleton<IAttachmentContentStorage, AttachmentContentStorage>();
            registry.Bind<ITranslationStorage, TranslationsStorage>();
            registry.BindAsSingleton<IPasswordHasher, DevicePasswordHasher>();
            registry.BindAsSingleton<IHttpStatistician, HttpStatistician>();

            registry.BindToMethod<IGeolocator>(() => CrossGeolocator.Current);
            registry.BindToMethod<IMedia>(() => CrossMedia.Current);
            registry.BindToMethod<IPermissions>(() => CrossPermissions.Current);

            registry.Bind<InterviewEntitiesListFragment>();
            registry.Bind<CompleteInterviewFragment>();
            registry.Bind<CoverInterviewFragment>();
            registry.Bind<OverviewFragment>();

            registry.BindAsSingleton<INearbyConnection, NearbyConnection>();
            registry.BindAsSingleton<INearbyCommunicator, NearbyCommunicator>();
            registry.BindAsSingleton<IRequestHandler, NearbyConnectionsRequestHandler>();
            registry.BindAsSingleton<IPayloadProvider, PayloadProvider>();
            registry.BindAsSingleton<IConnectionsApiLimits, ConnectionsApiLimits>();
        }

        public Task Init(IServiceLocator serviceLocator)
        {
            var requestHandler = serviceLocator.GetInstance<IRequestHandler>();
            var requestHandlers = serviceLocator.GetAllInstances<IHandleCommunicationMessage>();

            foreach (var handler in requestHandlers)
            {
                handler.Register(requestHandler);
            }

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            return Task.CompletedTask;
        }
    }
}
