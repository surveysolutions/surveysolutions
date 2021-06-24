using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.UI.Headquarters.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ControlPanelController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly ICommandService commandService;
        private readonly ILogger<ControlPanelController> logger;

        public ControlPanelController(
            IAuthorizedUser authorizedUser,
            ICommandService commandService,
            ILogger<ControlPanelController> logger)
        {
            this.authorizedUser = authorizedUser;
            this.commandService = commandService;
            this.logger = logger;
        }

        public ActionResult Index() => View();

        [ActivePage(MenuItem.Administration_Config)]
        public IActionResult Configuration() => View("Index");

        [ActivePage(MenuItem.Administration_Exceptions)]
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext);

        [ActivePage(MenuItem.Administration_AppUpdates)]
        public IActionResult AppUpdates() => View("Index");

        [ActivePage(MenuItem.Administration_InterviewPackages)]
        public IActionResult InterviewPackages() => this.View("Index");

        [HttpGet]
        [ActivePage(MenuItem.Administration_ReevaluateInterview)]
        public IActionResult ReevaluateInterview() => this.View("Index");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReevaluateInterview(Guid id)
        {
            try
            {
                this.commandService.Execute(new ReevaluateInterview(id, this.authorizedUser.Id, DateTimeOffset.UtcNow));
                return this.Ok();
            }
            catch (Exception exception)
            {
                this.logger.LogError($"Exception while reevaluatng: {id}", exception);
                return this.UnprocessableEntity(exception);
            }
        }

        [HttpGet]
        public IActionResult RaiseException(int? id = null)
        {
            throw new ArgumentException("Test exception");
        }
    }
}
