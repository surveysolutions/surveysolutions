using System;

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

        public void Error(string message, bool append = false) => this.WriteToTempData(Alerts.ERROR, message, append);

        public void Info(string message, bool append = false) => this.WriteToTempData(Alerts.INFO, message, append);

        public void Success(string message, bool append = false) => this.WriteToTempData(Alerts.SUCCESS, message, append);

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.UserHelper = UserHelper;
        }

        private void WriteToTempData(string key, string message, bool append = false)
        {
            if (this.TempData.ContainsKey(key))
            {
                if (append)
                {
                    this.TempData[key] += Environment.NewLine + message;
                }
                else
                {
                    this.TempData[key] = message;
                }
            }
            else
            {
                this.TempData.Add(key, message);
            }
        }
    }
}