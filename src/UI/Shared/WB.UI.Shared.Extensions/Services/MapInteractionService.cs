using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Shared.Extensions.Activities;
using WB.UI.Shared.Extensions.Entities;
using WB.UI.Shared.Extensions.ViewModels;
using Xamarin.Essentials;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.UI.Shared.Extensions.Services
{
    public class MapInteractionService : WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IMapInteractionService
    {
        public class EventAwaiter<TEventArgs>
        {
            #region Fields

            private TaskCompletionSource<TEventArgs> _eventArrived = new TaskCompletionSource<TEventArgs>();

            #endregion Fields

            #region Properties

            public Task<TEventArgs> Task { get; set; }

            public EventHandler<TEventArgs> Subscription => (s, e) => _eventArrived.TrySetResult(e);

            #endregion Properties
        }

        private readonly IPermissionsService permissions;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public MapInteractionService(IPermissionsService permissions,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.permissions = permissions;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public async Task<AreaEditResult> EditAreaAsync(EditAreaArgs args)
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<Permissions.StorageWrite>().ConfigureAwait(false);

            return await this.EditAreaImplAsync(args);
        }

        public async Task OpenMapDashboardAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<Permissions.StorageWrite>().ConfigureAwait(false);

            await this.viewModelNavigationService.NavigateToAsync<MapDashboardViewModel, MapDashboardViewModelArgs>(
                new MapDashboardViewModelArgs()).ConfigureAwait(false);
        }

        public void SetLicenseKey(string key)
        {
            ArcGISRuntimeEnvironment.SetLicense(key);
        }

        public void SetApiKey(string key)
        {
            ArcGISRuntimeEnvironment.ApiKey = key;
        }

        public bool DoesSupportMaps => true;

        private async Task<AreaEditResult> EditAreaImplAsync(EditAreaArgs args)
        {
            bool activityCreated = await this.viewModelNavigationService.NavigateToAsync<GeographyEditorViewModel, GeographyEditorViewModelArgs>(
                new GeographyEditorViewModelArgs
                {
                    Geometry = args.Area?.Geometry,
                    MapName = args.Area?.MapName,
                    RequestedGeometryType = args.GeometryType,
                    RequestedGeometryInputMode = args.RequestedGeometryInputMode,
                    RequestedAccuracy = args.RequestedAccuracy,
                    RequestedFrequency = args.RequestedFrequency,
                    GeographyNeighbors = args.GeographyNeighbors,
                    Title = args.Title,
                }).ConfigureAwait(false);

            if (activityCreated)
            {
                var tcs = new TaskCompletionSource<AreaEditResult>();
                
                GeographyEditorActivity.OnAreaEditCompleted = (AreaEditorResult editResult) =>
                {
                    tcs.TrySetResult(editResult == null
                        ? null
                        : new AreaEditResult
                        {
                            Geometry = editResult.Geometry,
                            MapName = editResult.MapName,
                            Area = editResult.Area,
                            Length = editResult.Length,
                            Coordinates = editResult.Coordinates,
                            DistanceToEditor = editResult.DistanceToEditor,
                            Preview = editResult.Preview,
                            NumberOfPoints = editResult.NumberOfPoints,
                            RequestedAccuracy = editResult.RequestedAccuracy,
                        });
                };
                return await tcs.Task;
            }

            return await Task.FromResult<AreaEditResult>(null);
        }
    }
}
