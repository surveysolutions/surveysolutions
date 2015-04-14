using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Settings;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LocalOrDevelopmentAccessOnly]
    public abstract class ControlPanelController : BaseController
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IBrokenSyncPackagesStorage brokenSyncPackagesStorage;
        private readonly ISettingsProvider settingsProvider;

        public ControlPanelController(IServiceLocator serviceLocator,
            IBrokenSyncPackagesStorage brokenSyncPackagesStorage,
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger,
            ISettingsProvider settingsProvider)
            : base(commandService: commandService, globalInfo: globalInfo, logger: logger)
        {
            this.serviceLocator = serviceLocator;
            this.brokenSyncPackagesStorage = brokenSyncPackagesStorage;
            this.settingsProvider = settingsProvider;
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
            return this.View(this.brokenSyncPackagesStorage.GetListOfUnhandledPackages());
        }

        public FileResult GetIncomingDataWithError(string packageId)
        {
            return this.File(this.brokenSyncPackagesStorage.GetUnhandledPackagePath(packageId), System.Net.Mime.MediaTypeNames.Application.Octet, packageId);
        }
        
        public ActionResult Settings()
        {
            IEnumerable<ApplicationSetting> settings = this.settingsProvider.GetSettings();
            return this.View(settings);
        }

        [NoTransaction]
        public ActionResult ReadSide()
        {
            return this.View();
        }

        public ActionResult RepeatLastInterviewStatus(Guid? interviewId)
        {
            if (!interviewId.HasValue)
            {
                return this.View();
            }
            else
            {
                this.ExecuteCommandWithObserverCheck(new RepeatLastInterviewStatus(interviewId.Value, "Status set by Survey Solutions support team"));
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

        public ActionResult InterviewDetails()
        {
            return this.View();
        }
    }
}