using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public ActionResult Index(QuestionnaireRevision id, string interviewId)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);
            if (questionnaireView == null)
                return NotFound();
            
            var token = this.webTesterService.CreateTestQuestionnaire(id.QuestionnaireId);
            string url = $"{webTesterSettings.BaseUri}/{token}?sid=" + interviewId;
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
