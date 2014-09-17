using System;
using System.ServiceModel;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Supervisor.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelController : Controller
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IIncomePackagesRepository incomePackagesRepository;

        public ControlPanelController(IServiceLocator serviceLocator, 
            IIncomePackagesRepository incomePackagesRepository)
        {
            this.serviceLocator = serviceLocator;
            this.incomePackagesRepository = incomePackagesRepository;
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

        [AllowAnonymous]
        public ActionResult Index()
        {
            return this.View();
        }

        [AllowAnonymous]
        public ActionResult NConfig()
        {
            return this.View();
        }

        public ActionResult Settings()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult IncomingDataWithErrors()
        {
            return this.View(incomePackagesRepository.GetListOfUnhandledPackages());
        }

        [AllowAnonymous]
        public FileResult GetIncomingDataWithError(Guid id)
        {
            return this.File(incomePackagesRepository.GetUnhandledPackagePath(id), System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [AllowAnonymous]
        public ActionResult ReadLayer()
        {
            return this.RedirectToActionPermanent("ReadSide");
        }

        [AllowAnonymous]
        public ActionResult ReadSide()
        {
            return this.View(this.ReadSideAdministrationService.GetAllAvailableHandlers());
        }

        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetReadSideStatus()
        {
            return this.ReadSideAdministrationService.GetReadableStatus();
        }

        [AllowAnonymous]
        public ActionResult RebuildReadSidePartially(string[] handlers)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers);
            this.TempData["CheckedHandlers"] = handlers;
            return this.RedirectToAction("ReadSide");
        }

        [AllowAnonymous]
        public ActionResult RebuildReadSide(int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync(skipEvents);

            return this.RedirectToAction("ReadSide");
        }

        [AllowAnonymous]
        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
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
            return View();
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

        [AllowAnonymous]
        public ActionResult Headquarters()
        {
            bool areHeadquartersFunctionsEnabled = bool.Parse(WebConfigurationManager.AppSettings["HeadquartersFunctionsEnabled"]);
            return this.View(areHeadquartersFunctionsEnabled);
        }

        public ActionResult InterviewDetails()
        {
            return this.View();
        }
    }
}