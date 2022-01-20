using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    [ResponseCache(NoStore = true)]
    [Route("api/questionnaire")]
    public class QuestionnaireApiController : Controller
    {
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;
        private readonly IOptions<WebTesterSettings> webTesterSettings;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly IChapterInfoViewFactory chapterInfoViewFactory;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IWebTesterService webTesterService;
        private const int MaxCountOfOptionForFileredCombobox = 200;
        public const int MaxVerificationErrors = 100;

        public QuestionnaireApiController(IChapterInfoViewFactory chapterInfoViewFactory,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireInfoFactory questionnaireInfoFactory,
            IOptions<WebTesterSettings> webTesterSettings,
            IWebTesterService webTesterService)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireInfoFactory = questionnaireInfoFactory;
            this.webTesterSettings = webTesterSettings;
            this.webTesterService = webTesterService;
        }

        public IActionResult Details(string id)
        {
            return Ok();
        }

        [HttpGet]
        [Route("get/{id}")]
        public IActionResult Get(QuestionnaireRevision id)
        {
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, User.GetId());

            if (questionnaireInfoView == null)
            {
                return NotFound(string.Format(ExceptionMessages.QuestionCannotBeFound , id));
            }

            return Ok(questionnaireInfoView);
        }

        [HttpGet]
        [Route("chapter/{id}")]
        public IActionResult Chapter(QuestionnaireRevision id, string chapterId)
        {
            var chapterInfoView = this.chapterInfoViewFactory.Load(id, groupId: chapterId);

            if (chapterInfoView == null)
            {
                return NotFound();
            }

            return Ok(chapterInfoView);
        }

        [HttpGet]
        [Route("EditVariable/{id}")]
        public IActionResult EditVariable(QuestionnaireRevision id, Guid variableId)
        {
            var variableView = this.questionnaireInfoFactory.GetVariableEditView(id, variableId);

            if (variableView == null) return NotFound(string.Format(ExceptionMessages.VariableWithIdWasNotFound, variableId, id));

            var result = Ok(new
            {
                Id = variableView.ItemId,
                Expression = variableView.VariableData.Expression,
                variable = variableView.VariableData.Name,
                TypeOptions = variableView.TypeOptions,
                Type = variableView.VariableData.Type,
                breadcrumbs = variableView.Breadcrumbs,
                label = variableView.VariableData.Label,
                variableView.VariableData.DoNotExport
            });

            return result;
        }

        [HttpGet]
        [Route("EditQuestion/{id}")]
        public IActionResult EditQuestion(QuestionnaireRevision id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            if (editQuestionView == null)
            {
                return NotFound();
            }
            
            if ((editQuestionView.IsFilteredCombobox == true || !string.IsNullOrWhiteSpace(editQuestionView.CascadeFromQuestionId))
                && editQuestionView.Options != null)
            {
                editQuestionView.WereOptionsTruncated = editQuestionView.Options.Length > MaxCountOfOptionForFileredCombobox;
                editQuestionView.Options = editQuestionView.Options.Take(MaxCountOfOptionForFileredCombobox).ToArray();   
            }

            return Ok(editQuestionView);
        }

        [HttpGet]
        [Route("EditGroup/{id}")]
        public IActionResult EditGroup(QuestionnaireRevision id, Guid groupId)
        {
            var editGroupView = this.questionnaireInfoFactory.GetGroupEditView(id, groupId);

            if (editGroupView == null)
            {
                return NotFound();
            }

            return Ok(editGroupView);
        }

        [HttpGet]
        [Route("EditRoster/{id}")]
        public IActionResult EditRoster(QuestionnaireRevision id, Guid rosterId)
        {
            var editRosterView = this.questionnaireInfoFactory.GetRosterEditView(id, rosterId);
            if (editRosterView == null)
            {
                return NotFound();
            }

            return Ok(editRosterView);
        }

        [HttpGet]
        [Route("EditStaticText/{id}")]
        public IActionResult EditStaticText(QuestionnaireRevision id, Guid staticTextId)
        {
            var staticTextEditView = this.questionnaireInfoFactory.GetStaticTextEditView(id, staticTextId);

            if (staticTextEditView == null)
            {
                return NotFound();
            }

            return Ok(staticTextEditView);
        }

        [HttpGet]
        [Route("Verify/{id}")]
        public IActionResult Verify(QuestionnaireRevision id)
        {
            var questionnaireView = this.questionnaireViewFactory.Load(id);

            if (questionnaireView == null)
            {
                return NotFound();
            }

            QuestionnaireVerificationMessage[] verificationMessagesAndWarning = 
                this.questionnaireVerifier.GetAllErrors(questionnaireView,true).ToArray();
            
            var verificationErrors = verificationMessagesAndWarning
                .Where(x => x.MessageLevel > VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors)
                .ToArray();

            var verificationWarnings = verificationMessagesAndWarning
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .Take(MaxVerificationErrors - verificationErrors.Length)
                .ToArray();

            var readOnlyQuestionnaire = new ReadOnlyQuestionnaireDocument(questionnaireView.Source);
            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, readOnlyQuestionnaire);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, readOnlyQuestionnaire);

            return Ok(new VerificationResult
            (
                errors : errors,
                warnings : warnings
            ));
        }

        [HttpGet]
        [Route("WebTest/{id:guid}")]
        public string WebTest(Guid id)
        {
            var token = this.webTesterService.CreateTestQuestionnaire(id);
            return $"{webTesterSettings.Value.BaseUri}/{token}";
        }

        [HttpGet]
        [Route("GetAllBrokenGroupDependencies/{id}")]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(QuestionnaireRevision id, Guid groupId)
        {
            return this.questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
        }

        [HttpGet]
        [Route("GetQuestionsEligibleForNumericRosterTitle/{id}")]
        public List<DropdownEntityView>? GetQuestionsEligibleForNumericRosterTitle(QuestionnaireRevision id, Guid rosterId, Guid rosterSizeQuestionId)
        {
            return this.questionnaireInfoFactory.GetQuestionsEligibleForNumericRosterTitle(id, rosterId, rosterSizeQuestionId);
        }
    }
}
