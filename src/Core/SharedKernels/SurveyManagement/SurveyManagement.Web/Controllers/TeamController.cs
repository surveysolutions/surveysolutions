using System;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class TeamController : BaseController
    {
        protected readonly IUserViewFactory userViewFactory;
        protected readonly IPasswordHasher passwordHasher;

        public TeamController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
            this.ViewBag.ActivePage = MenuItem.Teams;
        }

        protected UserView GetUserById(Guid id)
        {
            return this.userViewFactory.Load(new UserViewInputModel(id));
        }

        protected UserView GetUserByName(string userName)
        {
            return this.userViewFactory.Load(new UserViewInputModel(UserName: userName, UserEmail: null));
        }

        protected void CreateInterviewer(UserModel interviewer, Guid supervisorId)
        {
            CreateUser(user: interviewer, role: UserRoles.Operator, supervisorId: supervisorId);
        }

        protected void CreateSupervisor(UserModel supervisorUser)
        {
            CreateUser(user: supervisorUser, role: UserRoles.Supervisor);
        }

        protected void CreateHeadquarter(UserModel headquarterUser)
        {
            CreateUser(user: headquarterUser, role: UserRoles.Headquarter);
        }

        protected void UpdateAccount(UserView user, UserEditModel editModel)
        {
            this.CommandService.Execute(new ChangeUserCommand(publicKey: user.PublicKey, email: editModel.Email,
                roles: user.Roles.ToArray(),
                isLockedBySupervisor: this.GlobalInfo.IsSurepvisor ? editModel.IsLocked : user.IsLockedBySupervisor,
                isLockedByHQ: this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator ? editModel.IsLocked : user.IsLockedByHQ,
                passwordHash:
                    string.IsNullOrEmpty(editModel.Password)
                        ? user.Password
                        : passwordHasher.Hash(editModel.Password), userId: this.GlobalInfo.GetCurrentUser().Id));
        }

        private void CreateUser(UserModel user, UserRoles role, Guid? supervisorId = null)
        {
            this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(), userName: user.UserName,
                password: passwordHasher.Hash(user.Password), email: user.Email, isLockedBySupervisor: false,
                isLockedByHQ: user.IsLocked, roles: new[] {role},
                supervsor: supervisorId.HasValue ? this.GetUserById(supervisorId.Value).GetUseLight() : null));
        }
    }
}