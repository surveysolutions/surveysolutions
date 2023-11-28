using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/{controller}/{action}")]
    public class ExportSettingsApiController : ControllerBase
    {
        private readonly ILogger<ExportSettingsApiController> logger;
        private readonly IExportSettings exportSettings;
        private readonly ISystemLog auditLog;
        private readonly IExportServiceApi exportServiceApi;
        private readonly IInScopeExecutor<IExportServiceApi> exportService;
        private readonly IWorkspaceContextAccessor workspaceContextAccessor;

        public ExportSettingsApiController(ILogger<ExportSettingsApiController> logger, 
            IExportSettings exportSettings,
            ISystemLog auditLog,
            IExportServiceApi exportServiceApi,
            IInScopeExecutor<IExportServiceApi> exportService,
            IWorkspaceContextAccessor workspaceContextAccessor)
        {
            this.exportSettings = exportSettings;
            this.auditLog = auditLog;
            this.exportServiceApi = exportServiceApi;
            this.exportService = exportService;
            this.workspaceContextAccessor = workspaceContextAccessor;
            this.logger = logger;
        }

        [HttpGet]
        public ExportSettingsModel ExportSettings()
        {
            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return model;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<ExportSettingsModel>> ChangeState([FromBody]ChangeSettingsModel changeSettingsState)
        {
            if (await this.IsExistsDataExportInProgress())
                return StatusCode((int)HttpStatusCode.Forbidden, new {message = DataExport.ErrorThereAreRunningProcesses});

            ExportSettingsModel oldState = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (oldState.IsEnabled != changeSettingsState.EnableState)
            {
                this.exportSettings.SetEncryptionEnforcement(changeSettingsState.EnableState);
                await this.ClearExportData();
            }

            this.auditLog.ExportEncryptionChanged(changeSettingsState.EnableState);
            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return newExportSettingsModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<ExportSettingsModel>> RegeneratePassword()
        {
            if (await this.IsExistsDataExportInProgress())
                return StatusCode((int)HttpStatusCode.Forbidden,new {message = DataExport.ErrorThereAreRunningProcesses}); 

            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (model.IsEnabled)
            {
                this.exportSettings.RegeneratePassword();
                await this.ClearExportData();
            }


            this.logger.LogInformation("Export settings were changed by {User}. Encryption password was changed.", new {User = base.User.Identity.Name});

            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return newExportSettingsModel;
        }
        
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveExportCache()
        {
            if (await this.IsExistsDataExportInProgress())
                return StatusCode((int)HttpStatusCode.Forbidden,new {message = DataExport.RemoveExportCacheGeneratingFail});

            var status = await exportServiceApi.StatusDeleteTenant();
            if (status.Status == StopTenantStatus.Removing)
                return StatusCode((int)HttpStatusCode.Forbidden,new {message = DataExport.RemoveExportCacheGeneratingFail});

            Task.Run(RunClearExportData).Wait(TimeSpan.FromSeconds(2));

            return new JsonResult(new { Success = true });
        }

        private async Task RunClearExportData()
        {
            this.logger.LogInformation("Start to remove export cache by {User}.", new { User = base.User.Identity.Name });

            try
            {
                var status = await exportServiceApi.StatusDeleteTenant();
                if (status.Status != StopTenantStatus.Removing)
                {
                    var workspace = workspaceContextAccessor.CurrentWorkspace()?.Name;
                    await exportService.ExecuteAsync(async export =>
                    {
                        await export.DeleteTenant();
                    }, workspace);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Fail to remove Export service tenant.");
                throw;
            }

            try
            {
                await this.ClearExportData();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Fail to remove Export service data.");
                throw;
            }

            this.logger.LogInformation("End to remove export cache by {User}.", new { User = base.User.Identity.Name });
        }

        private Task ClearExportData()
        {
            return exportServiceApi.DeleteAll();
        }

        private async Task<bool> IsExistsDataExportInProgress()
        {
            return (await this.exportServiceApi.GetRunningExportJobs()).Any();
        }
        
        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StatusExportCache()
        {
            try
            {
                var status = await exportServiceApi.StatusDeleteTenant();
                return new JsonResult(new
                {
                    Success = true,
                    Status = status.Status,
                });
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Fail to check status of remove Export service tenant.");
                throw;
            }
            
        }
    }
}
