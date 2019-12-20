using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
