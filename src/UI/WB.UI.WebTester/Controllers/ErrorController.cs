using Microsoft.AspNetCore.Mvc;

namespace WB.UI.WebTester.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult QuestionnaireWithErrors() => View();

        [HttpGet("error/404")]
        public IActionResult Error404() => View("NotFound");
    }
}
