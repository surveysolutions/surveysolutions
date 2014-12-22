using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
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
        private readonly IIncomePackagesRepository incomePackagesRepository;
        private readonly ISettingsProvider settingsProvider;

        public ControlPanelController(IServiceLocator serviceLocator, 
            IIncomePackagesRepository incomePackagesRepository,
            ICommandService commandService, 
            IGlobalInfoProvider globalInfo, 
            ILogger logger,
            ISettingsProvider settingsProvider)
            : base(commandService: commandService, globalInfo: globalInfo, logger: logger)
        {
            this.serviceLocator = serviceLocator;
            this.incomePackagesRepository = incomePackagesRepository;
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
            return this.View(this.incomePackagesRepository.GetListOfUnhandledPackages());
        }

        public FileResult GetIncomingDataWithError(Guid id)
        {
            return this.File(this.incomePackagesRepository.GetUnhandledPackagePath(id), System.Net.Mime.MediaTypeNames.Application.Octet);
        }
        
        public ActionResult Settings()
        {
            IEnumerable<ApplicationSetting> settings = this.settingsProvider.GetSettings();
            return this.View(settings);
        }

        public ActionResult ReadSide()
        {
            return this.View();
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