using System.Web.Mvc;
using WB.UI.Shared.Web.Controllers;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    [Authorize]
    public class BaseController : BaseMessageDisplayController
    {
        protected readonly IMembershipUserService UserHelper;

        public BaseController(IMembershipUserService userHelper)
        {
            this.UserHelper = userHelper;
        }
        
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.UserHelper = UserHelper;
        }
    }
}