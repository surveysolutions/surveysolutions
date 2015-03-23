namespace WB.UI.Designer.Controllers
{
    using System.Web.Mvc;

    using WB.UI.Designer.BootstrapSupport;
    using WB.UI.Shared.Web.Membership;

    public class BaseController : Controller
    {
        protected readonly IMembershipUserService UserHelper;

        public BaseController(IMembershipUserService userHelper)
        {
            this.UserHelper = userHelper;
        }

        public void Error(string message)
        {
            this.WriteToTempData(Alerts.ERROR, message);
        }

        public void Success(string message)
        {
            this.WriteToTempData(Alerts.SUCCESS, message);
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.UserHelper = UserHelper;
        }

        private void WriteToTempData(string key, string message)
        {
            if (this.TempData.ContainsKey(key))
            {
                this.TempData[key] = message;
            }
            else
            {
                this.TempData.Add(key, message);
            }
        }
    }
}