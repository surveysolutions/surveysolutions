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
            WriteToTempData(Alerts.ATTENTION, message);
        }

        public void Success(string message)
        {
            WriteToTempData(Alerts.SUCCESS, message);
        }

        public void Information(string message)
        {
            WriteToTempData(Alerts.INFORMATION, message);
        }

        public void Error(string message)
        {
            WriteToTempData(Alerts.ERROR, message);
        }

        private void WriteToTempData(string key, string message)
        {
            if (TempData.ContainsKey(key))
            {
                TempData[key] = message;
            }
            else
            {
                TempData.Add(key, message);
            }
            
        }
    }
}
