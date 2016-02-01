using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IPasswordHasher passwordHasher;
        

        public ControlPanelController(
            IServiceLocator serviceLocator,
            IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IUserViewFactory userViewFactory,
            IPasswordHasher passwordHasher,
            ISettingsProvider settingsProvider,
            ITransactionManagerProvider transactionManagerProvider,
            IEventStoreApiService eventStoreApiService)
            : base(serviceLocator, brokenSyncPackagesStorage, commandService, globalInfo, logger, settingsProvider, transactionManagerProvider, eventStoreApiService)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
        }

        public ActionResult CreateHeadquarters()
        {
            return this.View(new UserModel());
        }

        public ActionResult CreateAdmin()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHeadquarters(UserModel model)
        {
            if (CreateUser(model, UserRoles.Headquarter))
                return this.RedirectToAction("LogOn", "Account");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(UserModel model)
        {
            if (CreateUser(model, UserRoles.Administrator))
                return this.RedirectToAction("LogOn", "Account");

            return View(model);
        }

        private bool CreateUser(UserModel model, UserRoles role)
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
                            isLockedByHQ: false, roles: new[] { role }, supervsor: null,
                        personName: model.PersonName,
                        phoneNumber: model.PhoneNumber));
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var userErrorMessage = string.Format("Error when creating user {0} in role {1}", model.UserName, role);
                        this.Error(userErrorMessage);
                        this.Logger.Error(userErrorMessage, ex);
                    }
                }
                else
                {
                    this.Error("User name already exists. Please enter a different user name.");
                }
            }
            return false;
        }

        public ActionResult ResetPrivilegedUserPassword()
        {
            return this.View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetUserPassword(UserModel model)
        {
            UserView userToCheck =
                this.userViewFactory.Load(new UserViewInputModel(UserName: model.UserName, UserEmail: null));
            if (userToCheck != null && !userToCheck.IsArchived)
            {
                try
                {
                    this.CommandService.Execute(new ChangeUserCommand(publicKey: userToCheck.PublicKey,
                        email: userToCheck.Email, isLockedByHQ: userToCheck.IsLockedByHQ,
                        isLockedBySupervisor: userToCheck.IsLockedBySupervisor,
                        passwordHash: passwordHasher.Hash(model.Password), userId: Guid.Empty,
                        personName: userToCheck.PersonName, phoneNumber: userToCheck.PhoneNumber));

                    this.Success(string.Format("Password for user '{0}' successfully changed", userToCheck.UserName));
                }
                catch (Exception ex)
                {
                    var userErrorMessage = "Error when updating password for user";
                    this.Error(userErrorMessage);
                    this.Logger.Error(userErrorMessage, ex);
                }
            }
            else
            {
                this.Error(string.Format("User '{0}' does not exists", model.UserName));
            }

            return View(model);
        }
    }
}