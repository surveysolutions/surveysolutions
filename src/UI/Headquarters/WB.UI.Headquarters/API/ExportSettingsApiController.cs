using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class ExportSettingsApiController : ApiController
    {
        private readonly ILogger logger;
        private readonly IExportSettings exportSettings;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IAuditLog auditLog;
        private readonly IExportServiceApi exportServiceApi;

        public ExportSettingsApiController(ILogger logger, 
            IExportSettings exportSettings,
            IDataExportProcessesService dataExportProcessesService,
            IAuditLog auditLog,
            IExportServiceApi exportServiceApi)
        {
            this.exportSettings = exportSettings;
            this.dataExportProcessesService = dataExportProcessesService;
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
        public async Task<HttpResponseMessage> ChangeState(ChangeSettingsModel changeSettingsState)
        {
            if (this.IsExistsDataExportInProgress())
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, message: DataExport.ErrorThereAreRunningProcesses);

            ExportSettingsModel oldState = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (oldState.IsEnabled != changeSettingsState.EnableState)
            {
                this.exportSettings.SetEncryptionEnforcement(changeSettingsState.EnableState);
                await this.ClearExportData();
            }

            this.auditLog.ExportEncriptionChanged(changeSettingsState.EnableState);
            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return Request.CreateResponse(newExportSettingsModel);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> RegeneratePassword()
        {
            if (this.IsExistsDataExportInProgress())
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, message: DataExport.ErrorThereAreRunningProcesses); 

            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (model.IsEnabled)
            {
                this.exportSettings.RegeneratePassword();
                await this.ClearExportData();
            }


            this.logger.Info($"Export settings were changed by {base.User.Identity.Name}. Encryption password was chagned.");

            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return Request.CreateResponse(newExportSettingsModel);
        }

        private Task ClearExportData()
        {
            return exportServiceApi.DeleteAll();
        }

        private bool IsExistsDataExportInProgress()
        {
            return this.dataExportProcessesService.GetRunningExportProcesses().Any();
        }
    }
}
