using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;
using WB.UI.Shared.Extensions.CustomServices.AreaEditor;
using WB.UI.Shared.Extensions.CustomServices.MapDashboard;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.UI.Shared.Extensions.CustomServices
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

        private readonly IPermissions permissions;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public MapInteractionService(IPermissions permissions,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.permissions = permissions;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public async Task<AreaEditResult> EditAreaAsync(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area, WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? geometryType)
        {
            await this.permissions.AssureHasPermissionOrThrow<LocationPermission>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<StoragePermission>().ConfigureAwait(false);

            return await this.EditAreaImplAsync(area, geometryType);
        }

        public async Task OpenMapDashboardAsync()
        {
            await this.permissions.AssureHasPermissionOrThrow<LocationPermission>().ConfigureAwait(false);
            await this.permissions.AssureHasPermissionOrThrow<StoragePermission>().ConfigureAwait(false);

            await this.viewModelNavigationService.NavigateToAsync<MapDashboardViewModel>().ConfigureAwait(false);
        }

        public void Init(string key)
        {
            ArcGISRuntimeEnvironment.SetLicense(key);
        }

        public bool DoesSupportMaps => true;

        private async Task<AreaEditResult> EditAreaImplAsync(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area, WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? geometryType)
        {
            bool activityCreated = await this.viewModelNavigationService.NavigateToAsync<AreaEditorViewModel, AreaEditorViewModelArgs>(
                new AreaEditorViewModelArgs
                {
                    Geometry = area?.Geometry,
                    MapName = area?.MapName,
                    RequestedGeometryType = geometryType
                }).ConfigureAwait(false);

            if (activityCreated)
            {
                var tcs = new TaskCompletionSource<AreaEditResult>();
                
                AreaEditorActivity.OnAreaEditCompleted = (AreaEditorResult editResult) =>
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
                            NumberOfPoints = editResult.NumberOfPoints
                        });
                };
                return await tcs.Task;
            }

            return await Task.FromResult<AreaEditResult>(null);
        }
    }
}
