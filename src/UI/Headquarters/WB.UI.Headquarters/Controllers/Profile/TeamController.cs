using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.AspNet.Identity;
using Resources;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    public class TeamController : BaseController
    {
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly HqUserManager userManager;
        
        public TeamController(ICommandService commandService, ILogger logger, IAuthorizedUser authorizedUser, HqUserManager userManager)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.userManager = userManager;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        protected async Task<IdentityResult> UpdateAccountAsync(UserEditModel editModel)
        {
            var appUser = await this.userManager.FindByIdAsync(editModel.Id);

            if(appUser == null)
                return IdentityResult.Failed(FieldsAndValidations.CannotUpdate_CurrentUserDoesNotExists);
            if(appUser.IsArchived)
                return IdentityResult.Failed(FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

            appUser.Email = editModel.Email ?? string.Empty; // default value for email field
            appUser.FullName = editModel.PersonName;
            appUser.PhoneNumber = editModel.PhoneNumber;
            appUser.IsLockedBySupervisor = editModel.IsLockedBySupervisor;
            appUser.IsLockedByHeadquaters = this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter
                ? editModel.IsLocked
                : appUser.IsLockedByHeadquaters;

            return await this.userManager.UpdateAsync(appUser);
        }

        protected async Task<IdentityResult> UpdateAccountPasswordAsync(UserEditModel editModel)
        {
            if (editModel.Password != null && editModel.Password == editModel.ConfirmPassword)
            {
                var appUser = await this.userManager.FindByIdAsync(editModel.Id);
                if (appUser == null)
                    return IdentityResult.Failed(FieldsAndValidations.CannotUpdate_CurrentUserDoesNotExists);
                if (appUser.IsArchived)
                    return IdentityResult.Failed(FieldsAndValidations.CannotUpdate_CurrentUserIsArchived);

                return await this.userManager.ChangePasswordAsync(appUser, editModel.Password);
            }
            
            return IdentityResult.Failed(FieldsAndValidations.ConfirmPasswordErrorMassage);
        }
        
        protected async Task<IdentityResult> CreateUserAsync(UserModel user, UserRoles role, Guid? supervisorId = null, bool? isLockedBySupervisor = null)
        {
            if (supervisorId != null)
            {
                var supervisor = await this.userManager.FindByIdAsync(supervisorId.Value);
                if (supervisor == null || !supervisor.IsInRole(UserRoles.Supervisor) || supervisor.IsArchivedOrLocked)
                {
                    return IdentityResult.Failed(HQ.SupervisorNotFound);
                }
            }

            var identityResult = await this.userManager.CreateUserAsync(new HqUser
            {
                Id = Guid.NewGuid(),
                IsLockedBySupervisor = isLockedBySupervisor ?? false,
                IsLockedByHeadquaters = user.IsLocked,
                FullName = user.PersonName,
                Email = user.Email ?? string.Empty, // default value for email field
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Profile = supervisorId.HasValue ? new HqUserProfile {SupervisorId = supervisorId} : null
            }, user.Password, role);

            return identityResult;
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ObserverNotAllowed]
        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
        public async Task<ActionResult> UpdatePassword([Bind(Prefix = "UpdatePassword")]UserEditModel model)
        {
            if (this.ModelState.IsValid)
            {
                var updateResult = await this.UpdateAccountPasswordAsync(model);

                if (updateResult.Succeeded)
                {
                    this.Success(Strings.HQ_AccountController_AccountPasswordChangedSuccessfullyFormat.FormatString(model.UserName));
                    return RedirectToAction("Cancel", model);
                }
                else
                    this.ModelState.AddModelError("UpdatePassword." + nameof(UserEditModel.Password), string.Join(@", ", updateResult.Errors));
            }

            return View("Edit", model);
        }

        public virtual ActionResult Cancel(Guid? id)
        {
            return RedirectToAction(@"Index");
        }
    }
}
