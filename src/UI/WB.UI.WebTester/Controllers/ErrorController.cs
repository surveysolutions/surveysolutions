using System.Web.Mvc;

namespace WB.UI.WebTester.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult QuestionnaireWithErrors() => View();
    }
}