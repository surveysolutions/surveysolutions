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

        
        public ActionResult NCalcToCSharp()
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
                var account = Membership.GetUser(model.UserName);
                if (account == null)
                {
                    this.Error(string.Format("Account '{0}' does not exists", model.UserName));
                }
                else
                {
                    if (model.MakeAdmin)
                    {
                        if (Roles.IsUserInRole(model.UserName, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' has administrator role", model.UserName));
                        }
                        else
                        {
                            Roles.AddUserToRole(account.ProviderUserKey.ToString(), UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully added", model.UserName));   
                        }
                    }
                    else
                    {
                        if (!Roles.IsUserInRole(model.UserName, UserHelper.ADMINROLENAME))
                        {
                            this.Error(string.Format("Account '{0}' is not in administrator role", model.UserName));
                        }
                        else
                        {
                            Roles.RemoveUserFromRole(model.UserName, UserHelper.ADMINROLENAME);
                            this.Success(string.Format("Administrator role for '{0}' successfully removed", model.UserName));    
                        }
                    }
                }
            }

            return this.View();
        }
    }
}