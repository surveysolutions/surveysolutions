using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DiagnosticsController : Controller
    {
        public DiagnosticsController()
        {
        }

        [ActivePage(MenuItem.TabletLogs)]
        public IActionResult Logs()
        {
            var model = new LogsModel();
            model.DataUrl = Url.Action("Table", "TabletLogsApi");
            return View(model);
        }
        
        [ActivePage(MenuItem.AuditLog)]
        public IActionResult AuditLog()
        {
            var model = new AuditLogModel();
            model.DataUrl = Url.Action("GetSystemLog", "AdminSettings");
            
            return View(model);
        }
    }

    public class LogsModel
    {
        public string DataUrl { get; set; }
    }
    
    public class AuditLogModel
    {
        public string DataUrl { get; set; }
    }
}
