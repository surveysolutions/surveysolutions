using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer;
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

        public WebTesterReloadController(
            IOptions<WebTesterSettings> webTesterSettings, 
            IQuestionnaireViewFactory questionnaireViewFactory,
            IWebTesterService webTesterService)
        {
            this.webTesterSettings = webTesterSettings.Value;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.webTesterService = webTesterService;
        }

        [QuestionnairePermissions]
        [AuthorizeOrAnonymousQuestionnaire]
        public ActionResult Index(QuestionnaireRevision id, string interviewId, string? target = null, string? hash = null)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();
            
            var token = this.webTesterService.CreateTestQuestionnaire(id.QuestionnaireId);
            string url = QueryHelpers.AddQueryString($"{webTesterSettings.BaseUri}/{token}", "sid", interviewId);
            var requestedTarget = GetRequestedTarget(target);
            var requestedHash = GetRequestedHash(hash);

            if (requestedTarget != null)
                url = QueryHelpers.AddQueryString(url, "target", requestedTarget);

            if (requestedHash != null)
                url = QueryHelpers.AddQueryString(url, "hash", requestedHash);

            return Redirect(url);
        }

        private static string? GetRequestedTarget(string? target)
        {
            if (string.IsNullOrWhiteSpace(target))
                return null;

            if (string.Equals(target, "/Cover", StringComparison.OrdinalIgnoreCase))
                return "/Cover";

            if (string.Equals(target, "/Complete", StringComparison.OrdinalIgnoreCase))
                return "/Complete";

            const string sectionPrefix = "/Section/";
            if (!target.StartsWith(sectionPrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var sectionId = target[sectionPrefix.Length..];
            if (!Guid.TryParse(sectionId, out var parsedSectionId))
                return null;

            return $"{sectionPrefix}{parsedSectionId:D}";
        }

        private static string? GetRequestedHash(string? hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return null;

            var questionId = hash.Trim().TrimStart('#');
            if (!Guid.TryParse(questionId, out var parsedQuestionId))
                return null;

            return $"#{parsedQuestionId:D}";
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == User.GetId() || User.IsAdmin())
                return true;
            return questionnaireView.SharedPersons.Any(x => x.UserId == User.GetId());
        }
    }
}
