using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class TeamController : BaseController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;

        public TeamController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IViewFactory<UserViewInputModel, UserView> userViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
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

        protected void CreateSupervisor(UserModel supervisor)
        {
            CreateUser(user: supervisor, role: UserRoles.Supervisor);
        }

        protected void UpdateSupervisorOrInterviewer(UserView user, UserEditModel editModel)
        {
            this.CommandService.Execute(new ChangeUserCommand(publicKey: user.PublicKey, email: editModel.Email,
                roles: user.Roles.ToArray(),
                isLockedBySupervisor: this.GlobalInfo.IsSurepvisor ? editModel.IsLocked : user.IsLockedBySupervisor,
                isLockedByHQ: this.GlobalInfo.IsHeadquarter ? editModel.IsLocked : user.IsLockedByHQ,
                passwordHash:
                    string.IsNullOrEmpty(editModel.Password)
                        ? user.Password
                        : SimpleHash.ComputeHash(editModel.Password), userId: this.GlobalInfo.GetCurrentUser().Id));
        }

        private void CreateUser(UserModel user, UserRoles role, Guid? supervisorId = null)
        {
            this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(), userName: user.UserName,
                password: SimpleHash.ComputeHash(user.Password), email: user.Email, isLockedBySupervisor: false,
                isLockedByHQ: user.IsLocked, roles: new[] {role},
                supervsor: supervisorId.HasValue ? this.GetUserById(supervisorId.Value).GetUseLight() : null));
        }
    }
}