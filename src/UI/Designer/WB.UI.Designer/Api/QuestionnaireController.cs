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
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Code;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class QuestionnaireController : ApiController
    {
        private readonly IVerificationErrorsMapper verificationErrorsMapper;
        private readonly IQuestionnaireVerifier questionnaireVerifier;
        private readonly IQuestionnaireInfoFactory questionnaireInfoFactory;

        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IChapterInfoViewFactory chapterInfoViewFactory;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private const int MaxCountOfOptionForFileredCombobox = 200;
        public const int MaxVerificationErrors = 100;

        public QuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireVerifier questionnaireVerifier,
            IVerificationErrorsMapper verificationErrorsMapper,
            IQuestionnaireInfoFactory questionnaireInfoFactory)
        {
            this.chapterInfoViewFactory = chapterInfoViewFactory;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.questionnaireVerifier = questionnaireVerifier;
            this.verificationErrorsMapper = verificationErrorsMapper;
            this.questionnaireInfoFactory = questionnaireInfoFactory;

        }

        [HttpGet]
        [CamelCase]
        public HttpResponseMessage Get(string id)
        {
            var questionnaireInfoView = questionnaireInfoViewFactory.Load(id);

            if (questionnaireInfoView == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
            }

            return Request.CreateResponse(HttpStatusCode.OK, questionnaireInfoView);
        }

        [HttpGet]
        [CamelCase]
        public IQuestionnaireItem Chapter(string id, string chapterId)
        {
            var chapterInfoView = chapterInfoViewFactory.Load(questionnaireId: id, groupId: chapterId);

            if (chapterInfoView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return chapterInfoView;
        }

        [HttpGet]
        [CamelCase]
        public NewEditQuestionView EditQuestion(string id, Guid questionId)
        {
            var editQuestionView = questionnaireInfoFactory.GetQuestionEditView(id, questionId);

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
            var editGroupView = questionnaireInfoFactory.GetGroupEditView(id, groupId);

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
            var editRosterView = questionnaireInfoFactory.GetRosterEditView(id, rosterId);

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
            var staticTextEditView = questionnaireInfoFactory.GetStaticTextEditView(id, staticTextId);

            if (staticTextEditView == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return staticTextEditView;
        }

        [HttpGet]
        [CamelCase]
        public VerificationErrors Verify(Guid id)
        {
            var questionnaireDocument = this.GetQuestionnaire(id).Source;
            QuestionnaireVerificationError[] verificationErrors = questionnaireVerifier.Verify(questionnaireDocument).Take(MaxVerificationErrors).ToArray();
            var errorsCount = verificationErrors.Length;
            VerificationError[] errors = verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, questionnaireDocument);

            return new VerificationErrors
            {
                Errors = errors,
                ErrorsCount = errorsCount
            };
        }

        [HttpGet]
        [CamelCase]
        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string id, Guid groupId)
        {
            return questionnaireInfoFactory.GetAllBrokenGroupDependencies(id, groupId);
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