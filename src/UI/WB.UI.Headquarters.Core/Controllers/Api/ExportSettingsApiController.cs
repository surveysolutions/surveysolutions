using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
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
using WB.UI.Headquarters.Resources;

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
            var retentionSetting = exportSettings.GetExportRetentionSettings();
            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), 
                this.exportSettings.GetPassword(),
                retentionSetting?.Enabled ?? false,
                retentionSetting?.DaysToKeep ?? null,
                retentionSetting?.CountToKeep ?? null);
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
            var retentionSetting = exportSettings.GetExportRetentionSettings();
            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword(),
                retentionSetting?.Enabled ?? false,
                retentionSetting?.DaysToKeep ?? null,
                retentionSetting?.CountToKeep ?? null);
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

            var retentionSetting = exportSettings.GetExportRetentionSettings();
            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword(),
                retentionSetting?.Enabled ?? false,
                retentionSetting?.DaysToKeep ?? null,
                retentionSetting?.CountToKeep ?? null);
            return newExportSettingsModel;
        }
        
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveExportCache()
        {
            if (await this.IsExistsDataExportInProgress())
                return StatusCode((int)HttpStatusCode.Forbidden,new {message = Settings.RemoveExportCacheGeneratingFail});

            var status = await exportServiceApi.DroppingTenantStatus();
            if (status.Status == DropTenantStatus.Removing)
                return StatusCode((int)HttpStatusCode.Forbidden,new {message = Settings.RemoveExportCacheGeneratingFail});

            Task.Run(RunClearExportData).Wait(TimeSpan.FromSeconds(2));

            return new JsonResult(new { Success = true });
        }

        private async Task RunClearExportData()
        {
            this.logger.LogInformation("Start to remove export cache by {User}.", new { User = base.User.Identity.Name });

            try
            {
                var status = await exportServiceApi.DroppingTenantStatus();
                if (status.Status != DropTenantStatus.Removing)
                {
                    var workspace = workspaceContextAccessor.CurrentWorkspace()?.Name;
                    await exportService.ExecuteAsync(async export =>
                    {
                        await export.DropTenant();
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
            return exportServiceApi.DeleteArchives();
        }

        private async Task<bool> IsExistsDataExportInProgress()
        {
            return (await this.exportServiceApi.GetRunningExportJobs()).Any();
        }
        
        [HttpGet]
        public async Task<IActionResult> StatusExportCache()
        {
            try
            {
                var status = await exportServiceApi.DroppingTenantStatus();
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
        
        private void UpdateRetentionSettings(Action<ExportRetentionSettings> updateAction)
        { 
            var retentionSetting = exportSettings.GetExportRetentionSettings();
            if (retentionSetting == null)
                retentionSetting = new ExportRetentionSettings();
            
            updateAction.Invoke(retentionSetting);
            
            exportSettings.SetExportRetentionSettings(retentionSetting.Enabled,
                retentionSetting.DaysToKeep,
                retentionSetting.CountToKeep);
        }
        
        public class RetentionLimitInDaysModel
        {
            [Range(1, 1000)]
            public int? RetentionLimitInDays { get; set; }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetRetentionLimitInDays([FromBody] RetentionLimitInDaysModel message)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateRetentionSettings(settings =>
            {
                settings.DaysToKeep = message.RetentionLimitInDays;
            });

            return Ok(new {sucess = true});
        }
        
        public class RetentionLimitCountModel
        {
            [Range(1, 100000)]
            public int? RetentionLimitCount { get; set; }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetRetentionLimitCount([FromBody] RetentionLimitCountModel message)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateRetentionSettings(settings =>
            {
                settings.CountToKeep = message.RetentionLimitCount;
            });

            return Ok(new {sucess = true});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<ExportSettingsModel>> ChangeRetentionState(
            [FromBody] ChangeSettingsModel changeSettingsState)
        {
            if (!ModelState.IsValid)
                return Ok(new {sucess = false});

            UpdateRetentionSettings(settings =>
            {
                settings.Enabled = changeSettingsState.EnableState;
            });
            
            return Ok(new {sucess = true});
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceRunRetentionPolicy()
        {
            var exportRetentionSettings = this.exportSettings.GetExportRetentionSettings();
            if (!exportRetentionSettings?.Enabled != true)
                return;
            
            //exportServiceApi calls to delete old exports
            await exportServiceApi.DeleteArchives(exportRetentionSettings.CountToKeep ,exportRetentionSettings.DaysToKeep);
            
            return Ok(new {sucess = true});
        }
    }
}
