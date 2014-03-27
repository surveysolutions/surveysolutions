using System.Web.Mvc;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Controllers
{
    public abstract class BaseController : Controller
    {
        protected BaseController()
        {
        }

        public void Attention(string message)
        {
            this.WriteToTempData(Alerts.ATTENTION, message);
        }

        public void Error(string message)
        {
            this.WriteToTempData(Alerts.ERROR, message);
        }

        public void Information(string message)
        {
            this.WriteToTempData(Alerts.INFORMATION, message);
        }

        public void Success(string message)
        {
            this.WriteToTempData(Alerts.SUCCESS, message);
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