using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [Authorize]
    [QuestionnairePermissions]
    [ApiNoCache]
    public class QuestionnaireController : ApiController
    {
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;

        private readonly IMembershipUserService userHelper;
        private readonly WebTesterSettings webTesterSettings;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IChapterInfoViewFactory chapterInfoViewFactory;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IWebTesterService webTesterService;
        private const int MaxCountOfOptionForFileredCombobox = 200;
        public const int MaxVerificationErrors = 100;

        public QuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IMembershipUserService userHelper, 
            WebTesterSettings webTesterSettings,
            IWebTesterService webTesterService)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireInfoFactory = questionnaireInfoFactory;

            this.userHelper = userHelper;
            this.webTesterSettings = webTesterSettings;
            this.webTesterService = webTesterService;
        }

        [HttpGet]
        [CamelCase]
        public HttpResponseMessage Get(string id)
        {
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, this.userHelper.WebUser.UserId);

            if (questionnaireInfoView == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format(ExceptionMessages.QuestionCannotBeFound , id));
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, questionnaireInfoView);
        }

        [HttpGet]
        [CamelCase]
        public NewChapterView Chapter(string id, string chapterId)
        {
            var chapterInfoView = this.chapterInfoViewFactory.Load(questionnaireId: id, groupId: chapterId);

            if (chapterInfoView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return chapterInfoView;
        }

        [HttpGet]
        [CamelCase]
        public HttpResponseMessage EditVariable(string id, Guid variableId)
        {
            var variableView = this.questionnaireInfoFactory.GetVariableEditView(id, variableId);

            if (variableView == null) return this.Request.CreateErrorResponse(HttpStatusCode.NotFound,
                string.Format(ExceptionMessages.VariableWithIdWasNotFound, variableId, id));

            var result = this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                Id = variableView.ItemId,
                Expression = variableView.VariableData.Expression,
                variable = variableView.VariableData.Name,
                TypeOptions = variableView.TypeOptions,
                Type = variableView.VariableData.Type,
                breadcrumbs = variableView.Breadcrumbs,
                label = variableView.VariableData.Label
            });

            return result;
        }

        [HttpGet]
        [CamelCase]
        public NewEditQuestionView EditQuestion(string id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            if (editQuestionView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            bool shouldTruncateOptions = editQuestionView.Type == QuestionType.SingleOption
                && (editQuestionView.IsFilteredCombobox == true || !string.IsNullOrWhiteSpace(editQuestionView.CascadeFromQuestionId))
                && editQuestionView.Options != null;

            if (shouldTruncateOptions)
            {
                editQuestionView.WereOptionsTruncated = editQuestionView.Options.Length > MaxCountOfOptionForFileredCombobox;
                editQuestionView.Options = editQuestionView.Options.Take(MaxCountOfOptionForFileredCombobox).ToArray();   
            }

            return editQuestionView;
        }

        [HttpGet]
        [CamelCase]
        public NewEditGroupView EditGroup(string id, Guid groupId)
        {
            var editGroupView = this.questionnaireInfoFactory.GetGroupEditView(id, groupId);

            if (editGroupView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return editGroupView;
        }

        [HttpGet]
        [CamelCase]
        public NewEditRosterView EditRoster(string id, Guid rosterId)
        {
            var editRosterView = this.questionnaireInfoFactory.GetRosterEditView(id, rosterId);

            if (editRosterView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return editRosterView;
        }

        [HttpGet]
        [CamelCase]
        public NewEditStaticTextView EditStaticText(string id, Guid staticTextId)
        {
            var staticTextEditView = this.questionnaireInfoFactory.GetStaticTextEditView(id, staticTextId);

            if (staticTextEditView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return staticTextEditView;
        }

        [HttpGet]
        [CamelCase]
        public VerificationResult Verify(Guid id)
        {
            var questionnaireView = this.GetQuestionnaire(id);
            QuestionnaireVerificationMessage[] verificationMessagesAndWarning = this.questionnaireVerifier.Verify(questionnaireView).ToArray();
            
            var verificationErrors = verificationMessagesAndWarning
                .Where(x => x.MessageLevel > VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors)
                .ToArray();

            var verificationWarnings = verificationMessagesAndWarning
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors - verificationErrors.Length)
                .ToArray();

            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, questionnaireView.Source);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, questionnaireView.Source);

            return new VerificationResult
            {
                Errors = errors,
                Warnings = warnings
            };
        }

        [HttpGet]
        [CamelCase]
        public WebTesterInfo WebTest(Guid id) => new WebTesterInfo
        {
            Token = this.webTesterService.CreateTestQuestionnaire(id),
            BaseUri = webTesterSettings.BaseUri
        };

        public class WebTesterInfo
        {
            public string BaseUri { get; set; }
            public string Token { get; set; }
        }

        [HttpGet]
        [CamelCase]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string id, Guid groupId)
        {
            return this.questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
        }

        [HttpGet]
        [CamelCase]
        public List<DropdownEntityView> GetQuestionsEligibleForNumericRosterTitle(string id, Guid rosterId, Guid rosterSizeQuestionId)
        {
            return this.questionnaireInfoFactory.GetQuestionsEligibleForNumericRosterTitle(id, rosterId, rosterSizeQuestionId);
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return questionnaire;
        }

    }
}