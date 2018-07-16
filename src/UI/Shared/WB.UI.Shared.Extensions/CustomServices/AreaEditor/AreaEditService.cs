using System;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Shared.Enumerator.Services;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    public class AreaEditService : WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IAreaEditService
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

        public AreaEditService(IPermissions permissions,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.permissions = permissions;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public static void RegisterLicence()
        {
            ArcGISRuntimeEnvironment.SetLicense("runtimeadvanced,1000,rud000017554,none,***REMOVED***");
        }

        public async Task<AreaEditResult> EditAreaAsync(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area, WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? geometryType)
        {
            await this.permissions.AssureHasPermission(Permission.Location);
            await this.permissions.AssureHasPermission(Permission.Storage);

            return await this.EditAreaImplAsync(area, geometryType);
        }

        private async Task<AreaEditResult> EditAreaImplAsync(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area, WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? geometryType)
        {
            var tcs = new TaskCompletionSource<AreaEditResult>();

            void AreaEditorActivityOnOnAreaEditCompleted(AreaEditorResult editResult)
            {
                AreaEditorActivity.OnAreaEditCompleted -= (AreaEditorActivityOnOnAreaEditCompleted);

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
            }

            AreaEditorActivity.OnAreaEditCompleted += (AreaEditorActivityOnOnAreaEditCompleted);

            await this.viewModelNavigationService.NavigateToAsync<AreaEditorViewModel, AreaEditorViewModelArgs>(new AreaEditorViewModelArgs
            {
                Geometry = area?.Geometry,
                MapName = area?.MapName,
                RequestedGeometryType = geometryType
            });

            return await tcs.Task;
        }
    }
}
