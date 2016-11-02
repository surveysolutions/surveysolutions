using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : BaseController
    {
        readonly ISettingsProvider settingsProvider;

        public ControlPanelController(IMembershipUserService userHelper, ISettingsProvider settingsProvider)
            : base(userHelper)
        {
            this.settingsProvider = settingsProvider;
        }

        public ActionResult Settings() => this.View(this.settingsProvider.GetSettings().OrderBy(setting => setting.Name));

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult MakeAdmin() => this.View();

        [HttpPost]
        public ActionResult MakeAdmin(MakeAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = Membership.GetUser(model.Login);
                if (account == null)
                {
                    this.Error(string.Format("Account '{0}' does not exists", model.Login));
                }
                else
                {
                    if (model.IsAdmin)
                    {
                        if (Roles.IsUserInRole(model.Login, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' has administrator role", model.Login));
                        }
                        else
                        {
                            Roles.AddUserToRole(account.ProviderUserKey.ToString(), UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully added", model.Login));   
                        }
                    }
                    else
                    {
                        if (!Roles.IsUserInRole(model.Login, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' is not in administrator role", model.Login));
                        }
                        else
                        {
                            Roles.RemoveUserFromRole(model.Login, UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully removed", model.Login));    
                        }
                    }
                }
            }

            return this.View();
        }
    }
}