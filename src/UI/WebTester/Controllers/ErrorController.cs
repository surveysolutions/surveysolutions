using Microsoft.AspNetCore.Mvc;

namespace WB.UI.WebTester.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult QuestionnaireWithErrors() => View();

        public IActionResult NotFound() => View();   
    }
}
