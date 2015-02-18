using System.Web.Mvc;
using System.Web.Security;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : BaseController
    {
        public ControlPanelController(IMembershipUserService userHelper)
            : base(userHelper)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult ReadSide()
        {
            return this.View();
        }

        public ActionResult ExpressionGeneration()
        {
            return this.View();
        }

        public ActionResult MakeAdmin()
        {
            return this.View();
        }

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