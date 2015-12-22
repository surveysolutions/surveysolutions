using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [RoutePrefix("api/interviewer/v1/questionnaires")]
    [ProtobufJsonSerializer]
    public class InterviewerQuestionnairesController : ApiController
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly ISerializer serializer;

        public InterviewerQuestionnairesController(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer)
        {
            this.questionnaireStore = questionnaireStore;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.serializer = serializer;
        }

        [HttpGet]
        [Route("census")]
        [WriteToSyncLog(SynchronizationLogType.GetCensusQuestionnaires)]
        public List<QuestionnaireIdentity> Census()
        {
            var query = new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            };

            var censusQuestionnaires = this.questionnaireBrowseViewFactory.Load(query).Items.Where(questionnaire => questionnaire.AllowCensusMode)
                .Select(questionnaire => new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version)).ToList();

            return censusQuestionnaires;
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}")]
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaire)]
        public QuestionnaireApiView Get(Guid id, int version)
        {
            var questionnaireDocumentVersioned = this.questionnaireStore.AsVersioned().Get(id.FormatGuid(), version);

            if (questionnaireDocumentVersioned == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return new QuestionnaireApiView()
            {
                QuestionnaireDocument = this.serializer.Serialize(questionnaireDocumentVersioned.Questionnaire),
                AllowCensus = this.questionnaireBrowseItemFactory.Load(new QuestionnaireItemInputModel(id, version)).AllowCensusMode
            };
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}/assembly")]
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAssembly)]
        public HttpResponseMessage GetAssembly(Guid id, int version)
        {
            if (!this.questionnareAssemblyFileAccessor.IsQuestionnaireAssemblyExists(id, version))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(File.OpenRead(this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(id, version)))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            return response;
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/logstate")]
        [WriteToSyncLog(SynchronizationLogType.QuestionnaireProcessed)]
        public void LogQuestionnaireAsSuccessfullyHandled(Guid id, int version)
        {
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/assembly/logstate")]
        [WriteToSyncLog(SynchronizationLogType.QuestionnaireAssemblyProcessed)]
        public void LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version)
        {
        }
    }
}