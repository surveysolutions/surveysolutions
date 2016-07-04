﻿using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
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

        protected void CreateInterviewer(UserModel interviewer, Guid supervisorId)
        {
            CreateUser(user: interviewer, role: UserRoles.Operator, supervisorId: supervisorId);
        }

        protected void CreateSupervisor(UserModel supervisorUser)
        {
            CreateUser(user: supervisorUser, role: UserRoles.Supervisor);
        }

        protected void CreateHeadquarters(UserModel headquartersUser)
        {
            CreateUser(user: headquartersUser, role: UserRoles.Headquarter);
        }

        protected void CreateObserver(UserModel observerUser)
        {
            CreateUser(user: observerUser, role: UserRoles.Observer);
        }

        protected void CreateApiWriterUser(UserModel apiUser)
        {
            CreateUser(user: apiUser, role: UserRoles.ApiUser);
        }

        protected void UpdateAccount(UserView user, UserEditModel editModel)
        {
            this.CommandService.Execute(new ChangeUserCommand(publicKey: user.PublicKey, 
                email: editModel.Email,
                isLockedBySupervisor: editModel.IsLockedBySupervisor,
                isLockedByHQ: this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator ? editModel.IsLocked : user.IsLockedByHQ,
                passwordHash:
                    string.IsNullOrEmpty(editModel.Password)
                        ? user.Password
                        : passwordHasher.Hash(editModel.Password), 
                personName:editModel.PersonName, 
                phoneNumber:editModel.PhoneNumber, 
                userId: this.GlobalInfo.GetCurrentUser().Id));
        }

        private void CreateUser(UserModel user, UserRoles role, Guid? supervisorId = null)
        {
            this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(), userName: user.UserName,
                password: passwordHasher.Hash(user.Password), email: user.Email, isLockedBySupervisor: false,
                isLockedByHQ: user.IsLocked, roles: new[] {role},
                supervsor: supervisorId.HasValue ? this.GetUserById(supervisorId.Value).GetUseLight() : null,
                personName: user.PersonName, phoneNumber: user.PhoneNumber));
        }
    }
}