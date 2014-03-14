using System.Web.Mvc;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}