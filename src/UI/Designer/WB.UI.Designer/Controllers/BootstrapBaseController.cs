using System.Web.Mvc;
using Main.Core.View;
using WB.UI.Designer.BootstrapSupport;

namespace WB.UI.Designer.Controllers
{
    public class BootstrapBaseController: BaseController
    {
        public BootstrapBaseController() : base(null)
        {   
        }

        protected BootstrapBaseController(IViewRepository repository):
            base(repository)
        {
        }

        public void Attention(string message)
        {
            TempData.Add(Alerts.ATTENTION, message);
        }

        public void Success(string message)
        {
            TempData.Add(Alerts.SUCCESS, message);
        }

        public void Information(string message)
        {
            TempData.Add(Alerts.INFORMATION, message);
        }

        public void Error(string message)
        {
            TempData.Add(Alerts.ERROR, message);
        }
    }
}
