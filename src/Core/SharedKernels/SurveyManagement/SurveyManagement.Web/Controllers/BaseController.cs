using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.Security;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private readonly ICommandService commandService;
        protected readonly IGlobalInfoProvider GlobalInfo;

        protected readonly ILogger Logger;

        protected BaseController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger)
        {
            this.commandService = commandService;
            this.GlobalInfo = globalInfo;
            this.Logger = logger;
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

        public bool ExecuteCommandWithObserverCheck(ICommand command, string origin = null)
        {
            if (User.Identity.IsObserver())
            {
                this.Error("You cannot perform any operation in observer mode.");
                return false;
            }
            
            commandService.Execute(command, origin);
            return true;
            
        }

        public void ExecuteCommandWithoutObserverCheck(ICommand command, string origin = null)
        {
            commandService.Execute(command, origin);
        }
    }
}