using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    public class AccountController : TeamController
    {
        private readonly IFormsAuthentication authentication;

        public AccountController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IFormsAuthentication authentication, IUserViewFactory userViewFactory,
            IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            this.authentication = authentication;
        }

        [HttpGet]
        public ActionResult LogOn()
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            return this.View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            this.ViewBag.ActivePage = MenuItem.Logon;
            if (this.ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, passwordHasher.Hash(model.Password)))
                {
                    bool isAdmin = Roles.IsUserInRole(model.UserName, UserRoles.Administrator.ToString());
                    bool isHeadquarter = Roles.IsUserInRole(model.UserName, UserRoles.Headquarter.ToString());
                    bool isSupervisor = Roles.IsUserInRole(model.UserName, UserRoles.Supervisor.ToString());

                    if (isHeadquarter || (isSupervisor && LegacyOptions.SupervisorFunctionsEnabled) || isAdmin)
                    {
                        this.authentication.SignIn(model.UserName, false);
                        
                        if (isSupervisor)
                        {
                            return this.RedirectToAction("Index", "Survey");
                        }
                        if (isHeadquarter)
                        {
                            return this.RedirectToAction("SurveysAndStatuses", "HQ");
                        }

                        if (isAdmin)
                        {
                            return this.RedirectToAction("Index", "Headquarter");
                        }
                    }

                    this.ModelState.AddModelError(string.Empty, "You have no access to this site. Contact your administrator.");
                }
                else
                    this.ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
            }

            return this.View(model);
        }

        public ActionResult LogOff()
        {
            this.authentication.SignOut();
            return this.Redirect("~/");
        }

        public ActionResult Manage()
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            var currentUser = GetUserById(GlobalInfo.GetCurrentUser().Id);

            if (currentUser == null || !(GlobalInfo.IsHeadquarter || GlobalInfo.IsAdministrator))
                throw new HttpException(404, string.Empty);

            return View(new UserEditModel() {Id = currentUser.PublicKey, Email = currentUser.Email});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;
            if (this.ModelState.IsValid)
            {
                this.UpdateAccount(user: GetUserById(GlobalInfo.GetCurrentUser().Id), editModel: model);
                this.Success(Core.SharedKernels.SurveyManagement.Web.Properties.Strings.HQ_AccountController_AccountUpdatedSuccessfully);
            }

            return this.View(model);
        }
    }
}