using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.DataExport;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
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

        public ExportSettingsApiController(ILogger<ExportSettingsApiController> logger, 
            IExportSettings exportSettings,
            ISystemLog auditLog,
            IExportServiceApi exportServiceApi)
        {
            this.exportSettings = exportSettings;
            this.auditLog = auditLog;
            this.exportServiceApi = exportServiceApi;
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

        private Task ClearExportData()
        {
            return exportServiceApi.DeleteAll();
        }

        private async Task<bool> IsExistsDataExportInProgress()
        {
            return (await this.exportServiceApi.GetRunningExportJobs()).Any();
        }
    }
}
