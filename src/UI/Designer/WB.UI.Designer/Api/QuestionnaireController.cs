﻿using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Main.Core.View;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
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
        private readonly IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView> questionnaireInfoViewFactory;

        public QuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory,
            IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView> questionnaireInfoViewFactory,
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

        public QuestionnaireInfoView Get(string id)
        {
            var questionnaireInfoView = questionnaireInfoViewFactory.Load(new QuestionnaireInfoViewInputModel() {QuestionnaireId = id});
            if (questionnaireInfoView == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
            }

            return questionnaireInfoView;
        }

        [HttpGet]
        public IQuestionnaireItem Chapter(string id, string chapterId)
        {
            var chapterInfoView = chapterInfoViewFactory.Load(questionnaireId: id, groupId: chapterId);
            if (chapterInfoView == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Chapter with id={0} cannot be found", chapterId));
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
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} or question with id={1} cannot be found", id, questionId));
            }
            return editQuestionView;
        }

        [HttpGet]
        [CamelCase]
        public NewEditGroupView EditGroup(string id, Guid qroupId)
        {
            var editGroupView = questionnaireInfoFactory.GetGroupEditView(id, qroupId);
            if (editGroupView == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} or group with id={1} cannot be found", id, qroupId));
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
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} or roster with id={1} cannot be found", id, rosterId));
            }
            return editRosterView;
        }

        [HttpGet]
        [CamelCase]
        public VerificationErrors Verify(Guid id)
        {
            var questionnaireDocument = this.GetQuestionnaire(id).Source;

            var verificationErrors = questionnaireVerifier.Verify(questionnaireDocument).ToArray();
            var errors = verificationErrorsMapper.EnrichVerificationErrors(verificationErrors, questionnaireDocument);
            var verificationResult = new VerificationErrors
            {
                Errors = errors
            };
            return verificationResult;
        }

        private QuestionnaireView GetQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));

            if (questionnaire == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, string.Format("Questionnaire with id={0} cannot be found", id));
            }

            return questionnaire;
        }

    }
}
