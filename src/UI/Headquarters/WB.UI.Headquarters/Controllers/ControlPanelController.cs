using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Storage.EventStore;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Headquarters.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : BaseController
    {
        private readonly IUserViewFactory userViewFactory;
        private readonly IPasswordHasher passwordHasher;
        private readonly IRestoreDeletedQuestionnaireProjectionsService restoreDeletedQuestionnaireProjectionsService;
        private readonly IServiceLocator serviceLocator;
        private readonly ISettingsProvider settingsProvider;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IEventStoreApiService eventStoreApiService;

        public ControlPanelController(
            IServiceLocator serviceLocator,
            ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IUserViewFactory userViewFactory,
            IPasswordHasher passwordHasher,
            ISettingsProvider settingsProvider,
            ITransactionManagerProvider transactionManagerProvider,
            IEventStoreApiService eventStoreApiService,
            IRestoreDeletedQuestionnaireProjectionsService restoreDeletedQuestionnaireProjectionsService)
             : base(commandService: commandService, globalInfo: globalInfo, logger: logger)
        {
            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
            this.restoreDeletedQuestionnaireProjectionsService = restoreDeletedQuestionnaireProjectionsService;
            this.serviceLocator = serviceLocator;
            this.settingsProvider = settingsProvider;
            this.transactionManagerProvider = transactionManagerProvider;
            this.eventStoreApiService = eventStoreApiService;
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

        public ActionResult RestoreAllDeletedQuestionnaireProjections()
        {
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RestoreAllDeletedQuestionnaireProjectionsPost()
        {
            this.restoreDeletedQuestionnaireProjectionsService.RestoreAllDeletedQuestionnaireProjections();
            return View("RestoreAllDeletedQuestionnaireProjections");
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

            return RedirectToAction("ResetPrivilegedUserPassword");
        }

        private IRevalidateInterviewsAdministrationService RevalidateInterviewsAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IRevalidateInterviewsAdministrationService>(); }
        }

        public ActionResult Index() => this.View();

        public ActionResult NConfig() => this.View();

        public ActionResult Versions() => this.View();

        public ActionResult Settings()
        {
            IEnumerable<ApplicationSetting> settings = this.settingsProvider.GetSettings().OrderBy(setting => setting.Name);
            return this.View(settings);
        }

        [NoTransaction]
        public ActionResult ReadSide() => this.View();

        public ActionResult RepeatLastInterviewStatus(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.View();
            }
            else
            {
                try
                {
                    this.CommandService.Execute(new RepeatLastInterviewStatus(interviewId.Value, Strings.ControlPanelController_RepeatLastInterviewStatus));
                }
                catch (Exception exception)
                {
                    Logger.Error(string.Format("Exception while repating last interview status: {0}", interviewId), exception);
                }

                return this.View(model: string.Format("Successfully repeated status for interview {0}", interviewId.Value.FormatGuid()));
            }
        }

        #region interview ravalidationg

        public ActionResult RevalidateInterviews()
        {
            return this.View();
        }

        public ActionResult RevalidateAllInterviewsWithErrors()
        {
            this.RevalidateInterviewsAdministrationService.RevalidateAllInterviewsWithErrorsAsync();

            return this.RedirectToAction("RevalidateInterviews");
        }

        public ActionResult StopInterviewRevalidating()
        {
            this.RevalidateInterviewsAdministrationService.StopInterviewsRevalidating();

            return this.RedirectToAction("RevalidateInterviews");
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetRevalidateInterviewStatus()
        {
            return this.RevalidateInterviewsAdministrationService.GetReadableStatus();
        }

        #endregion

        public ActionResult SynchronizationLog() => this.View();

        public ActionResult EventStore() => this.View();
        public ActionResult BrokenInterviewPackages() => this.View();

        public async Task<ActionResult> RunScavenge()
        {
            await eventStoreApiService.RunScavengeAsync();
            object model = "Scavenge has executed at " + DateTime.Now;
            return this.View("EventStore", model);
        }
    }
}