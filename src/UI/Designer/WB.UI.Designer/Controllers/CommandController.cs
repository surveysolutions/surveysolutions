namespace WB.UI.Designer.Controllers
{
    using System.Web.Mvc;

    using Ncqrs.Commanding;

    public class CommandController : Controller
    {
        [HttpPost]
        public ActionResult Execute(string command)
        {
            return new EmptyResult();
        }
    }
}