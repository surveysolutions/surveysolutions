using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WB.UI.WebTester.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult QuestionnaireWithErrors() => View();

        public IActionResult Error(int statusCode)
        {
            if (statusCode == StatusCodes.Status404NotFound)
                return View("NotFound");

            return StatusCode(statusCode);
        }
    }
}
