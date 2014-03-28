using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class QuestionnairesController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }
    }
}