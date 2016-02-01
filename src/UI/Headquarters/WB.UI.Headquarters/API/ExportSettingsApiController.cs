﻿using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Security;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.API
{
    [Authorize(Roles = "Administrator")]
    public class ExportSettingsApiController : ApiController
    {
        private readonly ILogger logger;
        private IExportSettings exportSettings;
        public ExportSettingsApiController(ILogger logger, IExportSettings exportSettings)

        {
            this.exportSettings = exportSettings;
            this.logger = logger;
        }

        [HttpGet]
        public ExportSettingsModel ExportSettings()
        {
            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            return model;
        }

        [HttpPost]
        public ExportSettingsModel ChangeState(ChangeSettingsModel changeSettingsState)
        {
            ExportSettingsModel oldState = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

                if (oldState.IsEnabled != changeSettingsState.EnableState)
                    this.exportSettings.SetEncryptionEnforcement(changeSettingsState.EnableState);
            
            return new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());
        }

        [HttpPost]
        public ExportSettingsModel RegeneratePassword()
        {
            ExportSettingsModel model = new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

            if(model.IsEnabled)
                this.exportSettings.RegeneratePassword();

            return new ExportSettingsModel(this.exportSettings.EncryptionEnforced(), this.exportSettings.GetPassword());

        }
    }
}
