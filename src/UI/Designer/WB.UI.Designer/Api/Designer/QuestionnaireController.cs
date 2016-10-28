using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Api
{
    [Authorize]
    [QuestionnairePermissions]
    public class QuestionnaireController : ApiController
    {
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;

        private readonly IMembershipUserService userHelper;

        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IChapterInfoViewFactory chapterInfoViewFactory;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private const int MaxCountOfOptionForFileredCombobox = 200;
        public const int MaxVerificationErrors = 100;

        public QuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IMembershipUserService userHelper)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireInfoFactory = questionnaireInfoFactory;

            this.userHelper = userHelper;
        }

        [HttpGet]
        [CamelCase]
        public HttpResponseMessage Get(string id)
        {
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, this.userHelper.WebUser.UserId);

            if (questionnaireInfoView == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Questionnaire with id={id} cannot be found");
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
                $"variable with id {variableId} was not found in questionnaire {id}");

            var result = this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                Id = variableView.ItemId,
                Expression = variableView.VariableData.Expression,
                name = variableView.VariableData.Name,
                TypeOptions = variableView.TypeOptions,
                Type = variableView.VariableData.Type,
                breadcrumbs = variableView.Breadcrumbs
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
            var questionnaireDocument = this.GetQuestionnaire(id).Source;
            QuestionnaireVerificationMessage[] verificationMessagesAndWarning = this.questionnaireVerifier.Verify(questionnaireDocument).ToArray();
            
            var verificationErrors = verificationMessagesAndWarning
                .Where(x => x.MessageLevel > VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors)
                .ToArray();

            var verificationWarnings = verificationMessagesAndWarning
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors - verificationErrors.Length)
                .ToArray();

            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, questionnaireDocument);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, questionnaireDocument);

            return new VerificationResult
            {
                Errors = errors,
                Warnings = warnings
            };
        }

        [HttpGet]
        [CamelCase]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string id, Guid groupId)
        {
            return this.questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
        }

        [HttpGet]
        [CamelCase]
        public List<DropdownQuestionView> GetQuestionsEligibleForNumericRosterTitle(string id, Guid rosterId, Guid rosterSizeQuestionId)
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