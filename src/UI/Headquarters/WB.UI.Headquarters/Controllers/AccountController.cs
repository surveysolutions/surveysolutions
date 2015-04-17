using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;
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
                    var userRoles = Roles.GetRolesForUser(model.UserName);

                    bool isAdmin = userRoles.Contains(UserRoles.Administrator.ToString(), StringComparer.OrdinalIgnoreCase);
                    bool isHeadquarter = userRoles.Contains(UserRoles.Headquarter.ToString(), StringComparer.OrdinalIgnoreCase);
                    bool isSupervisor = userRoles.Contains(UserRoles.Supervisor.ToString(), StringComparer.OrdinalIgnoreCase);
                    bool isObserver = userRoles.Contains(UserRoles.Observer.ToString(), StringComparer.OrdinalIgnoreCase);

                    if (isHeadquarter || (isSupervisor && LegacyOptions.SupervisorFunctionsEnabled) || isAdmin || isObserver)
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
                        if (isObserver || isAdmin)
                        {
                            return this.RedirectToAction("Index", "Headquarters");
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
        [ObserverNotAllowed]
        public ActionResult Manage(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.ManageAccount;

            if (this.ModelState.IsValid)
            {
                this.UpdateAccount(user: GetUserById(GlobalInfo.GetCurrentUser().Id), editModel: model);
                this.Success(Strings.HQ_AccountController_AccountUpdatedSuccessfully);
            }
            
            return this.View(model);
        }

        
        [Authorize(Roles = "Administrator, Observer")]
        public ActionResult ObservePerson(string personName)
        {
            if (string.IsNullOrEmpty(personName)) 
                throw new HttpException(404, string.Empty);
            
            var user = Membership.GetUser(personName);
            if (user == null) 
                throw new HttpException(404, string.Empty);
            
            var currentUser = GlobalInfo.GetCurrentUser().Name;

            var forbiddenRoles = new string[] { UserRoles.Administrator.ToString(), UserRoles.Observer.ToString(), UserRoles.Operator.ToString() };
            var userRoles = Roles.GetRolesForUser(user.UserName);
            bool invalidTargetUser = userRoles.Any(r => forbiddenRoles.Contains(r));
            
            if (invalidTargetUser) 
                throw new HttpException(404, string.Empty);
            bool isHeadquarter = userRoles.Contains(UserRoles.Headquarter.ToString(), StringComparer.OrdinalIgnoreCase);
            
            //do not forget pass currentuser to display you are observing
            this.authentication.SignIn(user.UserName, false, currentUser);
            
            return isHeadquarter ? 
                this.RedirectToAction("SurveysAndStatuses", "HQ") : 
                this.RedirectToAction("Index", "Survey");
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult ReturnToObserver()
        {
            var currentUserIdentity = (User.Identity as CustomIdentity);

            if (currentUserIdentity == null || string.IsNullOrEmpty(currentUserIdentity.ObserverName))
                throw new HttpException(404, string.Empty);
            
            var alowedRoles = new string[] { UserRoles.Administrator.ToString(), UserRoles.Observer.ToString(), UserRoles.Operator.ToString() };
            var userRoles = Roles.GetRolesForUser(currentUserIdentity.ObserverName);

            bool targetUserInValidRole = userRoles.Any(r => alowedRoles.Contains(r));

            if (!targetUserInValidRole) 
                throw new HttpException(404, string.Empty);
            
            this.authentication.SignIn(currentUserIdentity.ObserverName, false);
            return this.RedirectToAction("Index", "Headquarters");
        }
    }
}