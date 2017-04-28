using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Shared.Web.Controllers
{
    public abstract class BaseMessageDisplayController : Controller
    {
        public void Error(string message, bool append = false) => this.WriteToTempData(Alerts.ERROR, message, append);
        public void Info(string message, bool append = false) => this.WriteToTempData(Alerts.INFORMATION, message, append);
        public void Success(string message, bool append = false) => this.WriteToTempData(Alerts.SUCCESS, message, append);
        public void Attention(string message, bool append = false) => this.WriteToTempData(Alerts.ATTENTION, message, append);

        protected void WriteToTempData(string key, string message, bool append = false)
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

        protected void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }
}
