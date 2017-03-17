using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;

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
                return IdentityResult.Failed(@"Could not update user information because current user does not exist");

            appUser.Email = editModel.Email;
            appUser.FullName = editModel.PersonName;
            appUser.PhoneNumber = editModel.PhoneNumber;
            appUser.IsLockedBySupervisor = editModel.IsLockedBySupervisor;
            appUser.IsLockedByHeadquaters = this.authorizedUser.IsAdministrator ||
                                            this.authorizedUser.IsHeadquarter
                ? editModel.IsLocked
                : appUser.IsLockedByHeadquaters;

            return await this.userManager.UpdateUserAsync(appUser, editModel.Password);
        }

        protected async Task<IdentityResult> CreateUserAsync(UserModel user, UserRoles role, Guid? supervisorId = null)
            => await this.userManager.CreateUserAsync(new HqUser
            {
                Id = Guid.NewGuid(),
                IsLockedBySupervisor = false,
                IsLockedByHeadquaters = user.IsLocked,
                FullName = user.PersonName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Profile = supervisorId.HasValue ? new HqUserProfile {SupervisorId = supervisorId} : null
            }, user.Password, role);
    }
}