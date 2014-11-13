using System;
using System.Linq;
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
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public abstract class ControlPanelController : BaseController
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IIncomePackagesRepository incomePackagesRepository;

        public ControlPanelController(IServiceLocator serviceLocator, IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
            : base(commandService: commandService, globalInfo: globalInfo, logger: logger)
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

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NConfig()
        {
            return this.View();
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
            return this.View();
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

        public ActionResult RebuildReadSidePartially(string[] handlers, int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildViewsAsync(handlers, skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSidePartiallyForEventSources(string[] handlers, string eventSourceIds)
        {
            var sourceIds =
                eventSourceIds.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(e => Guid.Parse(e.Trim())).ToArray();
            this.ReadSideAdministrationService.RebuildViewForEventSourcesAsync(handlers, sourceIds);

            this.TempData["InProgress"] = true;
            this.TempData["CheckedHandlers"] = handlers;
            this.TempData["EventSources"] = eventSourceIds;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult RebuildReadSide(int skipEvents = 0)
        {
            this.ReadSideAdministrationService.RebuildAllViewsAsync(skipEvents);
            this.TempData["InProgress"] = true;
            this.TempData["SkipEvents"] = skipEvents;
            return this.RedirectToAction("ReadSide");
        }

        public ActionResult StopReadSideRebuilding()
        {
            this.ReadSideAdministrationService.StopAllViewsRebuilding();

            return this.RedirectToAction("ReadSide");
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

        public ActionResult InterviewDetails()
        {
            return this.View();
        }
    }
}