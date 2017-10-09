using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    [ObserverNotAllowed]
    [LimitsFilter]
    public class AuditLogController : BaseController
    {
        private readonly IAuditLogReader logReader;

        // GET
        public AuditLogController(ICommandService commandService, ILogger logger, IAuditLogReader logReader) : base(commandService, logger)
        {
            this.logReader = logReader;
        }

        [ActivePage(MenuItem.AuditLog)]
        public ActionResult Index()
        {
            var model = new AuditLogModel();
            model.ServerFilePathLocation = this.logReader.GetServerFilePath();
            model.Log = this.logReader.Read();
            return View(model);
        }
    }

    public class AuditLogModel
    {
        public string ServerFilePathLocation { get; set; }
        public string[] Log { get; set; }
    }
}