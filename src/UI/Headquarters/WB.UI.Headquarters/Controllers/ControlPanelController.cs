using System;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : WB.Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        private readonly IPasswordHasher passwordHasher;

        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory, IPasswordHasher passwordHasher, ISettingsProvider settingsProvider)
            : base(serviceLocator, incomePackagesRepository, commandService, globalInfo, logger, settingsProvider)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
        }

        public ActionResult CreateHeadquarters()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHeadquarters(UserModel model)
        {
            if (ModelState.IsValid)
            {
                UserView userToCheck =
                    this.userViewFactory.Load(new UserViewInputModel(UserName: model.UserName, UserEmail: null));
                if (userToCheck == null)
                {
                    try
                    {
                        this.CommandService.Execute(new CreateUserCommand(publicKey: Guid.NewGuid(),
                            userName: model.UserName,
                            password: passwordHasher.Hash(model.Password), email: model.Email,
                            isLockedBySupervisor: false,
                            isLockedByHQ: false, roles: new[] {UserRoles.Headquarter}, supervsor: null));
                        return this.RedirectToAction("LogOn", "Account");
                    }
                    catch (Exception ex)
                    {
                        var userErrorMessage = "Error when creating headquarters user";
                        this.Error(userErrorMessage);
                        this.Logger.Fatal(userErrorMessage, ex);
                    }
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }

            return View(model);
        }

        public ActionResult ResetHeadquartersPassword()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetHeadquartersPassword(UserModel model)
        {
            UserView userToCheck =
                this.userViewFactory.Load(new UserViewInputModel(UserName: model.UserName, UserEmail: null));
            if (userToCheck != null && userToCheck.Roles.Contains(UserRoles.Headquarter))
            {
                try
                {
                    this.CommandService.Execute(new ChangeUserCommand(publicKey: userToCheck.PublicKey,
                        email: userToCheck.Email, isLockedByHQ: userToCheck.IsLockedByHQ,
                        isLockedBySupervisor: userToCheck.IsLockedBySupervisor,
                        passwordHash: passwordHasher.Hash(model.Password), userId: Guid.Empty,
                        roles: userToCheck.Roles.ToArray()));
                    this.Success(string.Format("Password for headquarters '{0}' successfully changed",
                        userToCheck.UserName));
                }
                catch (Exception ex)
                {
                    var userErrorMessage = "Error when updating password for headquarters user";
                    this.Error(userErrorMessage);
                    this.Logger.Fatal(userErrorMessage, ex);
                }
            }
            else
            {
                this.Error(string.Format("Headquarters '{0}' does not exists", model.UserName));
            }

            return View(model);
        }
    }
}