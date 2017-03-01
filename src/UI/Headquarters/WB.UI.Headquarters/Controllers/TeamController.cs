using System;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using Microsoft.AspNet.Identity;
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
        protected readonly IIdentityManager identityManager;

        public TeamController(ICommandService commandService, ILogger logger, IIdentityManager identityManager)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        protected async Task<IdentityResult> UpdateAccountAsync(UserEditModel editModel)
        {
            var appUser = await this.identityManager.GetUserById(editModel.Id);

            appUser.Email = editModel.Email;
            appUser.FullName = editModel.PersonName;
            appUser.PhoneNumber = editModel.PhoneNumber;
            appUser.IsLockedBySupervisor = editModel.IsLockedBySupervisor;
            appUser.IsLockedByHeadquaters = this.identityManager.IsCurrentUserAdministrator ||
                                            this.identityManager.IsCurrentUserHeadquarter
                ? editModel.IsLocked
                : appUser.IsLockedByHeadquaters;

            return await this.identityManager.UpdateUserAsync(appUser, editModel.Password);
        }

        protected async Task<IdentityResult> CreateUserAsync(UserModel user, UserRoles role, Guid? supervisorId = null)
            => await this.identityManager.CreateUserAsync(new ApplicationUser
            {
                IsLockedBySupervisor = false,
                IsLockedByHeadquaters = user.IsLocked,
                FullName = user.PersonName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                SupervisorId = supervisorId
            }, user.Password, role);
    }
}