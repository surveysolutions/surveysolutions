using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Models;
using WB.UI.Designer.Services;
using WB.UI.Designer1.Extensions;

namespace WB.UI.Designer.Api
{
    [Authorize]
    [QuestionnairePermissions]
    [ResponseCache(NoStore = true)]
    public class QuestionnaireController : Controller
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

        public QuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory,
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

        [HttpGet]
        public IActionResult Get(string id)
        {
            var questionnaireInfoView = this.questionnaireInfoViewFactory.Load(id, User.GetId());

            if (questionnaireInfoView == null)
            {
                return NotFound(string.Format(ExceptionMessages.QuestionCannotBeFound , id));
            }

            return Ok(questionnaireInfoView);
        }

        [HttpGet]
        public IActionResult Chapter(string id, string chapterId)
        {
            var chapterInfoView = this.chapterInfoViewFactory.Load(questionnaireId: id, groupId: chapterId);

            if (chapterInfoView == null)
            {
                return NotFound();
            }

            return Ok(chapterInfoView);
        }

        [HttpGet]
        public IActionResult EditVariable(string id, Guid variableId)
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
                label = variableView.VariableData.Label
            });

            return result;
        }

        [HttpGet]
        public IActionResult EditQuestion(string id, Guid questionId)
        {
            var editQuestionView = this.questionnaireInfoFactory.GetQuestionEditView(id, questionId);

            if (editQuestionView == null)
            {
                return NotFound();
            }

            bool shouldTruncateOptions = editQuestionView.Type == QuestionType.SingleOption
                && (editQuestionView.IsFilteredCombobox == true || !string.IsNullOrWhiteSpace(editQuestionView.CascadeFromQuestionId))
                && editQuestionView.Options != null;

            if (shouldTruncateOptions)
            {
                editQuestionView.WereOptionsTruncated = editQuestionView.Options.Length > MaxCountOfOptionForFileredCombobox;
                editQuestionView.Options = editQuestionView.Options.Take(MaxCountOfOptionForFileredCombobox).ToArray();   
            }

            return Ok(editQuestionView);
        }

        [HttpGet]
        public IActionResult EditGroup(string id, Guid groupId)
        {
            var editGroupView = this.questionnaireInfoFactory.GetGroupEditView(id, groupId);

            if (editGroupView == null)
            {
                return NotFound();
            }

            return Ok(editGroupView);
        }

        [HttpGet]
        public IActionResult EditRoster(string id, Guid rosterId)
        {
            var editRosterView = this.questionnaireInfoFactory.GetRosterEditView(id, rosterId);

            if (editRosterView == null)
            {
                return NotFound();
            }

            return Ok(editRosterView);
        }

        [HttpGet]
        public IActionResult EditStaticText(string id, Guid staticTextId)
        {
            var staticTextEditView = this.questionnaireInfoFactory.GetStaticTextEditView(id, staticTextId);

            if (staticTextEditView == null)
            {
                return NotFound();
            }

            return Ok(staticTextEditView);
        }

        [HttpGet]
        public IActionResult Verify(Guid id)
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

            var readOnlyQuestionnaire = questionnaireView.Source.AsReadOnly();
            VerificationMessage[] errors = this.verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, readOnlyQuestionnaire);
            VerificationMessage[] warnings = this.verificationErrorsMapper.EnrichVerificationErrors(verificationWarnings, readOnlyQuestionnaire);

            return Ok(new VerificationResult
            {
                Errors = errors,
                Warnings = warnings
            });
        }

        [HttpGet]
        public string WebTest(Guid id)
        {
            var token = this.webTesterService.CreateTestQuestionnaire(id);
            return $"{webTesterSettings.Value.BaseUri}/{token}";
        }

        [HttpGet]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string id, Guid groupId)
        {
            return this.questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
        }

        [HttpGet]
        public List<DropdownEntityView> GetQuestionsEligibleForNumericRosterTitle(string id, Guid rosterId, Guid rosterSizeQuestionId)
        {
            return this.questionnaireInfoFactory.GetQuestionsEligibleForNumericRosterTitle(id, rosterId, rosterSizeQuestionId);
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            var questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new Exception("Not found");
            }

            return questionnaire;
        }
    }
}
