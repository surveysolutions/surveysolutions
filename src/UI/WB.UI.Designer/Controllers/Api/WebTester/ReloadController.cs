using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.WebTester
{
    public class WebTesterReloadController : ControllerBase
    {
        private readonly WebTesterSettings webTesterSettings;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IWebTesterService webTesterService;
        private readonly UserManager<DesignerIdentityUser> userManager;
        private readonly ILogger<WebTesterReloadController> logger;

        public WebTesterReloadController(
            IOptions<WebTesterSettings> webTesterSettings,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IWebTesterService webTesterService,
            UserManager<DesignerIdentityUser> userManager,
            ILogger<WebTesterReloadController> logger)
        {
            this.webTesterSettings = webTesterSettings.Value;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.webTesterService = webTesterService;
            this.userManager = userManager;
            this.logger = logger;
        }

        [QuestionnairePermissions]
        [AuthorizeOrAnonymousQuestionnaire]
        public async Task<ActionResult> Index(QuestionnaireRevision id, string interviewId)
        {
            var questionnaireView = questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();

            var userId = User.GetIdOrNull();
            DesignerIdentityUser? user = userId.HasValue
                ? await userManager.FindByIdAsync(userId.Value.ToString())
                : null;

            var correlationId = Guid.NewGuid().ToString("N");

            // Creates a one-time code — the JWT never reaches the browser.
            var code = await webTesterService.CreateOneTimeCodeAsync(
                id.QuestionnaireId,
                user?.Id.ToString(),
                correlationId);

            logger.LogInformation(
                "Redirecting to WebTester. CorrelationId={CorrelationId}, UserId={UserId}, " +
                "QuestionnaireId={QuestionnaireId}",
                correlationId, user?.Id.ToString() ?? "anonymous", id.QuestionnaireId);

            var url = $"{webTesterSettings.BaseUri}/{id.QuestionnaireId}" +
                      $"?code={Uri.EscapeDataString(code)}&sid={interviewId}";
            return Redirect(url);
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == User.GetId() || User.IsAdmin())
                return true;
            return questionnaireView.SharedPersons.Any(x => x.UserId == User.GetId());
        }
    }
}
