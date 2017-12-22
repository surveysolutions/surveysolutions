using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WB.UI.WebTester.Models;

namespace WB.UI.WebTester.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
