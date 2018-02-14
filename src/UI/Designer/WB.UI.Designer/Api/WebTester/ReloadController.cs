using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Api.WebTester
{
    public class WebTesterReloadController : Controller
    {
        private readonly IMembershipUserService userHelper;
        private readonly WebTesterSettings webTesterSettings;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IWebTesterService webTesterService;

        public WebTesterReloadController(IMembershipUserService userHelper, 
            WebTesterSettings webTesterSettings, 
            IQuestionnaireViewFactory questionnaireViewFactory,
            IWebTesterService webTesterService)
        {
            this.userHelper = userHelper;
            this.webTesterSettings = webTesterSettings;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.webTesterService = webTesterService;
        }

        [Authorize]
        public ActionResult Index(string id, string interviewId)
        {
            var questionnaireId = Guid.Parse(id);
            var questionnaireView = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
            {
                throw new HttpException(404, string.Empty);
            }

            if (!this.ValidateAccessPermissions(questionnaireView))
            {
                throw new HttpException(401, string.Empty);
            }

            var token = this.webTesterService.CreateTestQuestionnaire(questionnaireId);
            string url = $"{webTesterSettings.BaseUri}/{token}?sid=" + interviewId;
            return Redirect(url);
        }

        private bool ValidateAccessPermissions(QuestionnaireView questionnaireView)
        {
            if (questionnaireView.IsPublic || questionnaireView.CreatedBy == this.userHelper.WebUser.UserId || this.userHelper.WebUser.IsAdmin)
                return true;
            return questionnaireView.SharedPersons.Any(x => x.UserId == this.userHelper.WebUser.UserId);
        }
    }
}