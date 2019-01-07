using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;

namespace WB.UI.Designer.Controllers
{
    public class ClassificationsController : BaseController
    {
        public ClassificationsController(IMembershipUserService userHelper) : base(userHelper)
        {
        }

        [ValidateInput(false)]
        public ActionResult Index()
        {
            return this.View();
        }
    }
}
