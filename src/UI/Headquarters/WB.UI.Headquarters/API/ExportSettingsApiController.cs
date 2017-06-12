﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Resources;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Ddi;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class ExportSettingsApiController : ApiController
    {
        private readonly ILogger logger;
        private readonly IExportSettings exportSettings;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IDataExportProcessesService dataExportProcessesService;
        private readonly IDdiMetadataAccessor ddiMetadataAccessor;
        private readonly IParaDataAccessor paraDataAccessor;

        public ExportSettingsApiController(ILogger logger, 
            IExportSettings exportSettings,
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            IFileSystemAccessor fileSystemAccessor,
            IDataExportProcessesService dataExportProcessesService,
            IDdiMetadataAccessor ddiMetadataAccessor,
            IParaDataAccessor paraDataAccessor)

        {
            this.exportSettings = exportSettings;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.dataExportProcessesService = dataExportProcessesService;
            this.ddiMetadataAccessor = ddiMetadataAccessor;
            this.paraDataAccessor = paraDataAccessor;
            this.logger = logger;
        }

        [HttpGet]
        public ExportSettingsModel ExportSettings()
        {
            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            return model;
        }

        [HttpPost]
        public HttpResponseMessage ChangeState(ChangeSettingsModel changeSettingsState)
        {
            if (this.IsExistsDataExportInProgress())
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, message: DataExport.ErrorThereAreRunningProcesses);

            ExportSettingsModel oldState = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (oldState.IsEnabled != changeSettingsState.EnableState)
            {
                this.exportSettings.SetEncryptionEnforcement(changeSettingsState.EnableState);
                this.ClearExportData();
            }

            this.logger.Info($"Export settings were changed by {base.User.Identity.Name}. Encription changed to " + (changeSettingsState.EnableState ? "enabled" : "disabled"));

            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return Request.CreateResponse(newExportSettingsModel);
        }

        [HttpPost]
        public HttpResponseMessage RegeneratePassword()
        {
            if (this.IsExistsDataExportInProgress())
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, message: DataExport.ErrorThereAreRunningProcesses); 

            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if (model.IsEnabled)
            {
                this.exportSettings.RegeneratePassword();
                this.ClearExportData();
            }


            this.logger.Info($"Export settings were changed by {base.User.Identity.Name}. Encryption password was chagned.");

            var newExportSettingsModel = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
            return Request.CreateResponse(newExportSettingsModel);
        }

        private void ClearExportData()
        {
            var exportedDataDirectoryPath = this.filebasedExportedDataAccessor.GetExportDirectory();

            this.fileSystemAccessor.DeleteDirectory(exportedDataDirectoryPath);
            this.fileSystemAccessor.CreateDirectory(exportedDataDirectoryPath);

            this.ddiMetadataAccessor.ClearFiles();

            this.paraDataAccessor.ClearParaDataFile();
        }

        private bool IsExistsDataExportInProgress()
        {
            return this.dataExportProcessesService.GetRunningExportProcesses().Any();
        }
    }
}
