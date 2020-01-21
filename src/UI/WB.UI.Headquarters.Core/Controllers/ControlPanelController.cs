using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ControlPanelController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult TabletInfos()
        {
            return View("Index", new { DataUrl = Url.Action("TabletInfos", "ControlPanelApi")});
        }

        public IActionResult Configuration()
        {
            return View("Index");
        }
        
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);
    }
}
