using System;
using System.Web.Mvc;

using Newtonsoft.Json;
using Quartz;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.UI.Supervisor.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class HQSyncController : BaseController
    {
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly HeadquartersPushContext headquartersPushContext;
        private readonly IScheduler scheduler;
        private readonly ISynchronizer synchronizer;
        private readonly IGlobalInfoProvider globalInfoProvider;

        public HQSyncController(ICommandService commandService, ILogger logger, HeadquartersPullContext headquartersPullContext, HeadquartersPushContext headquartersPushContext, IScheduler scheduler, ISynchronizer synchronizer, IGlobalInfoProvider globalInfoProvider)
            : base(commandService, globalInfoProvider, logger)
        {
            this.headquartersPullContext = headquartersPullContext;
            this.headquartersPushContext = headquartersPushContext;
            this.scheduler = scheduler;
            this.synchronizer = synchronizer;
            this.globalInfoProvider = globalInfoProvider;
        }

        public ActionResult Synchronization()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Synchronize()
        {
            this.scheduler.TriggerJob(new JobKey("HQ sync", "Synchronization"));
            return Json(new object());
        }

        [HttpPost]
        public ActionResult Push()
        {
            Guid userId = this.globalInfoProvider.GetCurrentUser().Id;

            this.synchronizer.Push(userId);

            return Json(new object());
        }

        [NoTransaction]
        public ActionResult PullStatus()
        {
            return Json(new
            {
                Status = this.headquartersPullContext.GetStatus(),
                Messages = this.headquartersPullContext.GetMessages(),
                Errors = this.headquartersPullContext.GetErrors(),
                IsRunning = this.headquartersPullContext.IsRunning,
            }, JsonRequestBehavior.AllowGet);
        }

        [NoTransaction]
        public ActionResult PushStatus()
        {
            return Json(new
            {
                Status = this.headquartersPushContext.GetStatus(),
                Messages = this.headquartersPushContext.GetMessages(),
                Errors = this.headquartersPushContext.GetErrors(),
                IsRunning = this.headquartersPushContext.IsRunning,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSyncStatus()
        {
            SynchronizationStatus synchronizationStatus = this.headquartersPullContext.GetPersistedStatus();

            var result = synchronizationStatus ?? new SynchronizationStatus();
            return this.Content(JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore
            }));
        }
    }
}