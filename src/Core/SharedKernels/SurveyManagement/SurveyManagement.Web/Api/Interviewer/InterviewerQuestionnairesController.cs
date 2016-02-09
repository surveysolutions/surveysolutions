using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Factories;
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
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly ISerializer serializer;

        public InterviewerQuestionnairesController(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer)
        {
            this.questionnaireStore = questionnaireStore;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.serializer = serializer;
        }

        [HttpGet]
        [Route("census")]
        [WriteToSyncLog(SynchronizationLogType.GetCensusQuestionnaires)]
        public HttpResponseMessage Census()
        {
            var query = new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            };

            var censusQuestionnaires = this.questionnaireBrowseViewFactory.Load(query).Items.Where(questionnaire => questionnaire.AllowCensusMode)
                .Select(questionnaire => new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version)).ToList();

            var response = this.Request.CreateResponse(censusQuestionnaires);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };
            return response;
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}")]
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaire)]
        [Obsolete]
        public HttpResponseMessage Get(Guid id, int version)
        {
            return Get(id, version, 11);
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}/{contentVersion:long}")]
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaire)]
        public HttpResponseMessage Get(Guid id, int version, long contentVersion)
        {
            var questionnaireDocumentVersioned = this.questionnaireStore.AsVersioned().Get(id.FormatGuid(), version);

            if (questionnaireDocumentVersioned == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            if (contentVersion < questionnaireDocumentVersioned.QuestionnaireContentVersion)
            {
                return Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            var resultValue = new QuestionnaireApiView
            {
                QuestionnaireDocument = this.serializer.Serialize(questionnaireDocumentVersioned.Questionnaire),
                AllowCensus = this.questionnaireBrowseViewFactory.GetById(new QuestionnaireIdentity(id, version)).AllowCensusMode
            };

            var response = Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
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
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

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