using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.UI.Designer.BootstrapSupport;

namespace WB.UI.Designer.Controllers
{
    public class AlertController: BaseController
    {
        public AlertController() : base(null, null)
        {   
        }

        protected AlertController(IViewRepository repository, ICommandService commandService) :
            base(repository, commandService)
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
