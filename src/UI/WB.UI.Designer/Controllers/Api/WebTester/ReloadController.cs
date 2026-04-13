using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IJwtTokenService jwtTokenService;
        private readonly UserManager<DesignerIdentityUser> userManager;

        public WebTesterReloadController(
            IOptions<WebTesterSettings> webTesterSettings, 
            IQuestionnaireViewFactory questionnaireViewFactory,
            IJwtTokenService jwtTokenService,
            UserManager<DesignerIdentityUser> userManager)
        {
            this.webTesterSettings = webTesterSettings.Value;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.jwtTokenService = jwtTokenService;
            this.userManager = userManager;
        }

        [QuestionnairePermissions]
        [AuthorizeOrAnonymousQuestionnaire]
        public async Task<ActionResult> Index(QuestionnaireRevision id, string interviewId)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();

            var jwtToken = await GenerateWebTesterJwtAsync(id.QuestionnaireId);
            string url = $"{webTesterSettings.BaseUri}/{id.QuestionnaireId}?sid={interviewId}&jwt={Uri.EscapeDataString(jwtToken)}";
            return Redirect(url);
        }

        private async Task<string> GenerateWebTesterJwtAsync(Guid questionnaireId)
        {
            var userId = User.GetIdOrNull();
            DesignerIdentityUser? user = userId.HasValue
                ? await userManager.FindByIdAsync(userId.Value.ToString())
                : null;
            return jwtTokenService.GenerateWebTesterToken(user, questionnaireId);
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == User.GetId() || User.IsAdmin())
                return true;
            return questionnaireView.SharedPersons.Any(x => x.UserId == User.GetId());
        }
    }
}
