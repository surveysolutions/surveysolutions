using Microsoft.AspNetCore.Mvc;

namespace WB.UI.WebTester.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult QuestionnaireWithErrors() => View();

        [Route("error/404")]
        public new IActionResult NotFound() => View();   
    }
}
