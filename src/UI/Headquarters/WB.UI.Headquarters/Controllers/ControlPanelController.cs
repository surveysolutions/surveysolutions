using System;
using System.Linq;
using System.ServiceModel;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Headquarters.PublicService;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : WB.Core.SharedKernels.SurveyManagement.Web.Controllers.ControlPanelController
    {
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        private readonly IPasswordHasher passwordHasher;

        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory, IPasswordHasher passwordHasher)
            : base(serviceLocator, incomePackagesRepository, commandService, globalInfo, logger)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
        }

        public ActionResult Designer()
        {
            return this.Designer(null, null);
        }

        [HttpPost]
        public ActionResult Designer(string login, string password)
        {
            return this.View(model: this.DiagnoseDesignerConnection(login, password));
        }
   
        private string DiagnoseDesignerConnection(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                return "Please provide login and password to continue...";

            var service = new PublicServiceClient();

            service.ClientCredentials.UserName.UserName = login;
            service.ClientCredentials.UserName.Password = password;

            try
            {
                service.Dummy();

                return string.Format("Login to {0} succeeded!", service.Endpoint.Address.Uri);
            }
            catch (Exception exception)
            {
                return string.Format("Login to {1} failed.{0}{0}{2}", Environment.NewLine,
                    service.Endpoint.Address.Uri,
                    FormatDesignerConnectionException(exception));
            }
        }

        private static string FormatDesignerConnectionException(Exception exception)
        {
            var faultException = exception.InnerException as FaultException;

            if (faultException != null)
                return string.Format("Fault code: [ predefined: {1}, sender: {2}, receiver: {3}, subcode: {4}, name: {5} ]{0}{0}{6}", Environment.NewLine,
                    faultException.Code.IsPredefinedFault,
                    faultException.Code.IsSenderFault,
                    faultException.Code.IsReceiverFault,
                    faultException.Code.SubCode,
                    faultException.Code.Name,
                    exception);

            return exception.ToString();
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