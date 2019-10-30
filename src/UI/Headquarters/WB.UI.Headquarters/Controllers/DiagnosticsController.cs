using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers
{
    [AuthorizeOr403(Roles = "Administrator")]
    [LimitsFilter]
    public class DiagnosticsController : BaseController
    {
        public DiagnosticsController(ICommandService commandService, ILogger logger) : base(commandService, logger)
        {
        }

        public ActionResult Logs()
        {
            var model = new LogsModel();
            model.DataUrl = Url.RouteUrl("DefaultApiWithAction",
                new {httproute = "", controller = "TabletLogsApi", action = "Table"});
            return View(model);
        }
    }

    public class LogsModel
    {
        public string DataUrl { get; set; }
    }
}
