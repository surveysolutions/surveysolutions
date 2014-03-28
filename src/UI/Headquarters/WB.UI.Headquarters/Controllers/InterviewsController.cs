using System.Threading.Tasks;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Authentication;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = ApplicationRoles.Headquarter)]
    public class InterviewsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}