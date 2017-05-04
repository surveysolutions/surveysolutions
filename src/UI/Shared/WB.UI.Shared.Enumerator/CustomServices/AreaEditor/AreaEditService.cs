using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Esri.ArcGISRuntime;
using MvvmCross.Platform.Droid.Platform;
using Plugin.Permissions.Abstractions;
using RuntimeCoreNet.GeneratedWrappers;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Infrastructure.Shared.Enumerator;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    internal class AreaEditService : IAreaEditService
    {
        private readonly IMvxAndroidCurrentTopActivity androidCurrentTopActivity;
        private readonly IPermissions permissions;
        private IViewModelNavigationService viewModelNavigationService;

        public AreaEditService(IMvxAndroidCurrentTopActivity androidCurrentTopActivity, 
            IPermissions permissions,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.androidCurrentTopActivity = androidCurrentTopActivity;
            this.permissions = permissions;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        public async Task<AreaEditResult> EditAreaAsync(Area area)
        {
            await this.permissions.AssureHasPermission(Permission.Location);
            await this.permissions.AssureHasPermission(Permission.Storage);

            ArcGISRuntimeEnvironment.SetLicense("runtimebasic,1000,rud000017554,none,4N400PJPXJGH2T8AG192");

            return await this.EditArea(area);
        }

        private Task<AreaEditResult> EditArea(Area area)
        {
            return Task.Factory.StartNew<AreaEditResult>((Func<AreaEditResult>)(() =>
            {
                try
                {
                    AreaEditorResult result = null;
                    ManualResetEvent waitEditAreaResetEvent = new ManualResetEvent(false);
                    
                    viewModelNavigationService.NavigateToAreaEditor(area?.Geometry, area?.MapName, area?.AreaSize);
                    AreaEditorActivity.OnAreaEditCompleted += (editResult =>
                    {
                        result = editResult;
                        waitEditAreaResetEvent.Set();
                    });

                    waitEditAreaResetEvent.WaitOne();

                    return result != null ? new AreaEditResult() { Geometry = result.Geometry, MapName = result.MapName, Area = result.Area} : null;
                }
                catch (Exception)
                {
                    return (AreaEditResult)null;
                }
            }));
        }
    }
}