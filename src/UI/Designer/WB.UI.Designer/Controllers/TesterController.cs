using System.Web.Mvc;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class TesterController : BaseController
    {
        private const string appLink = "https://play.google.com/store/apps/details?id=org.worldbank.solutions.Vtester";

        public TesterController(IMembershipUserService userHelper)
            : base(userHelper) {}

        [AllowAnonymous]
        public ActionResult Index()
        {
            return this.RedirectPermanent(appLink);
        }
    }
}
