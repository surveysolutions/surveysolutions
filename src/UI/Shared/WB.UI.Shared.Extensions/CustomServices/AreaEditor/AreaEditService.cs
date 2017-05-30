using System;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Plugin.Permissions.Abstractions;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Infrastructure.Shared.Enumerator;
using AreaEditResult = WB.Core.SharedKernels.Enumerator.Services.Infrastructure.AreaEditResult;

namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    public class AreaEditService : WB.Core.SharedKernels.Enumerator.Services.Infrastructure.IAreaEditService
    {
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
            await this.permissions.AssureHasPermission(Permission.Location);
            await this.permissions.AssureHasPermission(Permission.Storage);

            ArcGISRuntimeEnvironment.SetLicense("runtimebasic,1000,rud000017554,none,4N400PJPXJGH2T8AG192");

            return await this.EditArea(area);
        }

        private Task<AreaEditResult> EditArea(WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area area)
        {
            return Task.Factory.StartNew<AreaEditResult>((Func<AreaEditResult>)(() =>
            {
                try
                {
                    AreaEditorResult result = null;
                    ManualResetEvent waitEditAreaResetEvent = new ManualResetEvent(false);

                    this.viewModelNavigationService.NavigateTo<AreaEditorViewModel>(new
                    {
                        geometry = area?.Geometry,
                        mapName = area?.MapName,
                        areaSize = area?.AreaSize
                    });
                    
                    AreaEditorActivity.OnAreaEditCompleted += (editResult =>
                    {
                        result = editResult;
                        waitEditAreaResetEvent.Set();
                    });

                    waitEditAreaResetEvent.WaitOne();

                    return result == null 
                        ? null 
                        : new AreaEditResult(){
                            Geometry = result.Geometry,
                            MapName = result.MapName,
                            Area = result.Area,
                            Length = result.Length,
                            DistanceToEditor = result.DistanceToEditor};
                }
                catch (Exception)
                {
                    return (AreaEditResult)null;
                }
            }));
        }
    }
}