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
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);

            return await this.EditAreaImplAsync(args);
        }

        public async Task OpenInterviewerMapDashboardAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);

            await this.viewModelNavigationService.NavigateToAsync<InterviewerMapDashboardViewModel, MapDashboardViewModelArgs>(
                new MapDashboardViewModelArgs()).ConfigureAwait(false);
        }

        public async Task OpenSupervisorMapDashboardAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            await this.permissions.AssureHasExternalStoragePermissionOrThrow().ConfigureAwait(false);

            await this.viewModelNavigationService.NavigateToAsync<SupervisorMapDashboardViewModel, MapDashboardViewModelArgs>(
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

        private Task<AreaEditResult> EditAreaImplAsync(EditAreaArgs args)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var waitScanResetEvent = new System.Threading.ManualResetEvent(false);
                
                this.viewModelNavigationService.NavigateToAsync<GeographyEditorViewModel, GeographyEditorViewModelArgs>(
                    new GeographyEditorViewModelArgs
                    {
                        Geometry = args.Area?.Geometry,
                        MapNameForGivenAnswer = args.Area?.MapName,
                        RequestedGeometryType = args.GeometryType,
                        RequestedGeometryInputMode = args.RequestedGeometryInputMode,
                        RequestedAccuracy = args.RequestedAccuracy,
                        RequestedFrequency = args.RequestedFrequency,
                        GeographyNeighbors = args.GeographyNeighbors,
                        Title = args.Title,
                    }).ConfigureAwait(false);

                AreaEditResult areaResult = null;
                GeographyEditorActivity.OnAreaEditCompleted = (AreaEditorResult editResult) =>
                {
                    areaResult = editResult == null
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
                                RequestedAccuracy = args.RequestedAccuracy,
                                RequestedFrequency = args.RequestedFrequency
                            };
                        waitScanResetEvent.Set();
                };
                waitScanResetEvent.WaitOne();
                return areaResult;
            });

            return task;
        }
    }
}
