using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Services;
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
    public class ControlPanelController : BaseController
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IIncomePackagesRepository incomePackagesRepository;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        private readonly IPasswordHasher passwordHasher;
        private static string lastReexportMessage = "no reexport performed";
        private readonly IDataExportRepositoryWriter dataExportRepositoryWriter;
        private readonly IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor;

        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<UserViewInputModel, UserView> userViewFactory, IPasswordHasher passwordHasher,
            IDataExportRepositoryWriter dataExportRepositoryWriter, IReadSideRepositoryIndexAccessor readSideRepositoryIndexAccessor)
            : base(commandService: commandService, globalInfo: globalInfo, logger: logger)
        {
            this.serviceLocator = serviceLocator;
            this.incomePackagesRepository = incomePackagesRepository;

            this.userViewFactory = userViewFactory;
            this.passwordHasher = passwordHasher;
            this.dataExportRepositoryWriter = dataExportRepositoryWriter;
            this.readSideRepositoryIndexAccessor = readSideRepositoryIndexAccessor;
        }

        /// <remarks>
        /// Getting dependency via service location ensures that parts of control panel not using it will always work.
        /// E.g. If Raven connection fails to be established then NConfig info still be available.
        /// </remarks>
        private IReadSideAdministrationService ReadSideAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IReadSideAdministrationService>(); }
        }

        private IRevalidateInterviewsAdministrationService RevalidateInterviewsAdministrationService
        {
            get { return this.serviceLocator.GetInstance<IRevalidateInterviewsAdministrationService>(); }
        }

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NConfig()
        {
            return this.View();
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

        public ActionResult IncomingDataWithErrors()
        {
            return this.View(this.incomePackagesRepository.GetListOfUnhandledPackages());
        }

        public FileResult GetIncomingDataWithError(Guid id)
        {
            return this.File(this.incomePackagesRepository.GetUnhandledPackagePath(id), System.Net.Mime.MediaTypeNames.Application.Octet);
        }
        
        public ActionResult Settings()
        {
            return View();
        }

        public ActionResult ReadLayer()
        {
            return this.RedirectToActionPermanent("ReadSide");
        }

        public ActionResult ReadSide()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadSideStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        public ActionResult RebuildReadSidePartially(string[] handlers)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers);
            this.TempData["CheckedHandlers"] = handlers;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSide(int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync(skipEvents);

            return this.RedirectToAction("ReadSide");
        }

        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
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

        #region reexport

        public ActionResult ReexportInterviews()
        {
            return this.View();
        }

        public ActionResult StartReexportApprovedInterviews(int? skip)
        {
            new Task(() => this.ReexportApprovedInterviewsImpl(skip ?? 0)).Start();

            return this.RedirectToAction("ReexportInterviews");
        }

        private void ReexportApprovedInterviewsImpl(int skip)
        {
            string indexName = typeof(InterviewsSearchIndex).Name;
            int pageSize = 20;

            int count = this.GetApprovedInterviewIds(indexName).Count();
            int processed = skip;

            lastReexportMessage = string.Format("found {0} interviews", count);
            while (processed < count)
            {
                List<Guid> interviewIds = this.GetApprovedInterviewIds(indexName).Skip(processed).Take(pageSize).ToList();

                foreach (var interviewId in interviewIds)
                {
                    this.dataExportRepositoryWriter.AddExportedDataByInterview(interviewId);
                    processed++;

                    lastReexportMessage = string.Format("last processed interview index: {0}", processed);
                }
            }
        }

        private IQueryable<Guid> GetApprovedInterviewIds(string indexName)
        {
            return this.readSideRepositoryIndexAccessor.Query<SeachIndexContent>(indexName)
                .Where(x => x.Status == InterviewStatus.ApprovedByHeadquarters || x.Status == InterviewStatus.ApprovedBySupervisor)
                .Select(x => x.InterviewId);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReexportStatus()
        {
            return lastReexportMessage;
        }

        #endregion

        public ActionResult InterviewDetails()
        {
            return this.View();
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