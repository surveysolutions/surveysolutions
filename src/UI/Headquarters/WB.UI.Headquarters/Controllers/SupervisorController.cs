using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{

    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter, Observer")]
    public class SupervisorController : TeamController
    {
        public SupervisorController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher,
                              IIdentityManager identityManager)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            this.IdentityManager = identityManager;
        }

        protected readonly IIdentityManager IdentityManager;

        public ActionResult Create()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Create(UserModel model)
        {

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            try
            {
                this.CreateSupervisor(model);
            }
            catch (Exception e)
            {
                this.Error(e.Message);
                return this.View(model);
            }
               
            this.Success("Supervisor was successfully created");
            return this.RedirectToAction("Index");
            

        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Index()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter, Observer")]
        public ActionResult Archived()
        {
            return this.View();
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult Edit(Guid id)
        {
            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    UserName = user.UserName,
                    PersonName = user.PersonName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Headquarter")]
        [ObserverNotAllowed]
        public ActionResult Edit(UserEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }
            
            var user = this.GetUserById(model.Id);
            if (user == null)
            {
                this.Error("Could not update user information because current user does not exist");
                return this.View(model);
            }
            var forbiddenRoles = new string[] {UserRoles.Administrator.ToString(), UserRoles.Headquarter.ToString()};
            var doesUserInForbiddenRole = IdentityManager.GetRolesForUser(user.UserName).Any(r => forbiddenRoles.Contains(r));

            if (doesUserInForbiddenRole)
            {
                this.Error("Could not update user information because you don't have permission to perform this operation");
                return this.View(model);
            }

            this.UpdateAccount(user: user, editModel: model);
            
            this.Success(string.Format("Information about <b>{0}</b> successfully updated", user.UserName));
            return this.RedirectToAction("Index");
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public ActionResult Interviewers(Guid id)
        {
            var supervisor = this.GetUserById(id);
            if (supervisor == null)
                throw new HttpException(404, string.Empty);

            return this.View(supervisor);
        }
    }
}