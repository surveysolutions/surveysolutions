using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using Quartz;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class HqSyncController : BaseController
    {
        private readonly SynchronizationContext synchronizationContext;
        private readonly IScheduler scheduler;
        private readonly ISynchronizer synchronizer;

        public HqSyncController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger, SynchronizationContext synchronizationContext, IScheduler scheduler, ISynchronizer synchronizer)
            : base(commandService, globalInfo, logger)
        {
            this.synchronizationContext = synchronizationContext;
            this.scheduler = scheduler;
            this.synchronizer = synchronizer;
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
            this.synchronizer.Push();
            return Json(new object());
        }

        public ActionResult SynchronizationStatus()
        {
            return Json(new
            {
                Status = this.synchronizationContext.GetStatus(),
                Messages = this.synchronizationContext.GetMessages(),
                Errors = this.synchronizationContext.GetErrors(),
                IsRunning = this.synchronizationContext.IsRunning,
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSyncStatus()
        {
            SynchronizationStatus synchronizationStatus = this.synchronizationContext.GetPersistedStatus();

            var result = synchronizationStatus ?? new SynchronizationStatus();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}