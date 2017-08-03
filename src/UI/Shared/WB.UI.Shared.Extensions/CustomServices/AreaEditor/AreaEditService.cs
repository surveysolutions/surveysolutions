using System;
using System.Threading;
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
        private IViewModelNavigationService viewModelNavigationService;

        public AreaEditService(IPermissions permissions,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.permissions = permissions;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public async Task<AreaEditResult> EditAreaAsync(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area)
        {
            bool is64Bit = IntPtr.Size == 8;

            if (is64Bit)
                throw new NotSupportedException("This functionality is not available for this device");

            await this.permissions.AssureHasPermission(Permission.Location);
            await this.permissions.AssureHasPermission(Permission.Storage);

            ArcGISRuntimeEnvironment.SetLicense("runtimebasic,1000,rud000017554,none,4N400PJPXJGH2T8AG192");

            return await this.EditArea(area);
        }
        
        private Task<AreaEditResult> EditArea(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area)
        {
            var tcs = new TaskCompletionSource<AreaEditResult>();

            this.viewModelNavigationService.NavigateTo<AreaEditorViewModel>(new
            {
                geometry = area?.Geometry,
                mapName = area?.MapName
            });

            AreaEditorActivity.OnAreaEditCompleted += (editResult =>
            {
                tcs.TrySetResult(
                    editResult == null
                        ? null
                        : new AreaEditResult()
                        {
                            Geometry = editResult.Geometry,
                            MapName = editResult.MapName,
                            Area = editResult.Area,
                            Length = editResult.Length,
                            Coordinates = editResult.Coordinates,
                            DistanceToEditor = editResult.DistanceToEditor,
                            Preview = editResult.Preview
                        });
            });

            return tcs.Task;
        }
    }
}