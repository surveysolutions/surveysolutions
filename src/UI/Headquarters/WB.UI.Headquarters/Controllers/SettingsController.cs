using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Security;

namespace WB.UI.Headquarters.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator")]
    [ObserverNotAllowed]
    public class SettingsController : BaseController
    {
        private ICypherManager cypherManager;
        public SettingsController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, ICypherManager cypherManager)
            : base(commandService, globalInfo, logger)
        {
            this.cypherManager = cypherManager;
        }

        public ActionResult DataEncryption()
        {
            this.ViewBag.ActivePage = MenuItem.Settings;

            CypherSettingsModel model = new CypherSettingsModel(this.cypherManager.EncryptionEnforced(), this.cypherManager.GetPassword());

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DataEncryption(CypherSettingsModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Settings;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            CypherSettingsModel oldState = new CypherSettingsModel(this.cypherManager.EncryptionEnforced(), this.cypherManager.GetPassword());

            if(oldState.IsEnabled != model.IsEnabled)
                this.cypherManager.SetEncryptionEnforcement(model.IsEnabled);

            return View(new CypherSettingsModel(this.cypherManager.EncryptionEnforced(), this.cypherManager.GetPassword()));
        }
    }
}