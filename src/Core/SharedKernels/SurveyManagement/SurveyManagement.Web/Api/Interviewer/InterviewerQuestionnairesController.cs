using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth]
    [RoutePrefix("api/interviewer/v1/questionnaires")]
    [ProtobufJsonSerializer]
    public class InterviewerQuestionnairesController : ApiController
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;
        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;
        private readonly IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IJsonUtils jsonUtils;
        private readonly ISyncLogger syncLogger;
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserWebViewFactory userInfoViewFactory;

        public InterviewerQuestionnairesController(
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore,
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem> questionnaireBrowseItemFactory,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IJsonUtils jsonUtils,
            ISyncLogger syncLogger,
            IGlobalInfoProvider globalInfoProvider,
            IUserWebViewFactory userInfoViewFactory)
        {
            this.questionnaireStore = questionnaireStore;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
            this.questionnaireBrowseItemFactory = questionnaireBrowseItemFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.jsonUtils = jsonUtils;
            this.syncLogger = syncLogger;
            this.globalInfoProvider = globalInfoProvider;
            this.userInfoViewFactory = userInfoViewFactory;
        }

        [HttpGet]
        [Route("census")]
        public List<QuestionnaireIdentity> Census()
        {
            var query = new QuestionnaireBrowseInputModel()
            {
                Page = 1,
                PageSize = int.MaxValue
            };

            var censusQuestionnaires = this.questionnaireBrowseViewFactory.Load(query).Items.Where(questionnaire => questionnaire.AllowCensusMode)
                .Select(questionnaire => new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version)).ToList();

            var deviceId = this.GetInterviewerDeviceId();
            var userId = this.globalInfoProvider.GetCurrentUser().Id;

            this.syncLogger.TrackArIdsRequest(deviceId, userId, SyncItemType.Questionnaire,
                censusQuestionnaires.Select(x => GetSyncLogQuestionnaireId(x.QuestionnaireId, 
                    x.Version, SyncItemType.Questionnaire)).ToArray());

            this.syncLogger.TrackArIdsRequest(deviceId, userId, SyncItemType.QuestionnaireAssembly,
                censusQuestionnaires.Select(x => GetSyncLogQuestionnaireId(x.QuestionnaireId,
                    x.Version, SyncItemType.QuestionnaireAssembly)).ToArray());

            return censusQuestionnaires;
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}")]
        public QuestionnaireApiView Get(Guid id, int version)
        {
            var questionnaireDocumentVersioned = this.questionnaireStore.AsVersioned().Get(id.FormatGuid(), version);

            if (questionnaireDocumentVersioned == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            this.syncLogger.TrackPackageRequest(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, SyncItemType.Questionnaire, GetSyncLogQuestionnaireId(id, version, SyncItemType.Questionnaire));

            return new QuestionnaireApiView()
            {
                QuestionnaireDocument = this.jsonUtils.Serialize(questionnaireDocumentVersioned.Questionnaire),
                AllowCensus = this.questionnaireBrowseItemFactory.Load(new QuestionnaireItemInputModel(id, version)).AllowCensusMode
            };
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}/assembly")]
        public HttpResponseMessage GetAssembly(Guid id, int version)
        {
            if (!this.questionnareAssemblyFileAccessor.IsQuestionnaireAssemblyExists(id, version))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(File.OpenRead(this.questionnareAssemblyFileAccessor.GetFullPathToAssembly(id, version)))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            
            this.syncLogger.TrackPackageRequest(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, SyncItemType.QuestionnaireAssembly, GetSyncLogQuestionnaireId(id, version, SyncItemType.QuestionnaireAssembly));

            return response;
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/logstate")]
        public void LogQuestionnaireAsSuccessfullyHandled(Guid id, int version)
        {
            this.syncLogger.MarkPackageAsSuccessfullyHandled(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, GetSyncLogQuestionnaireId(id, version, SyncItemType.Questionnaire));
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/assembly/logstate")]
        public void LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version)
        {
            this.syncLogger.MarkPackageAsSuccessfullyHandled(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, GetSyncLogQuestionnaireId(id, version, SyncItemType.QuestionnaireAssembly));
        }

        private Guid GetInterviewerDeviceId()
        {
            return this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null)).DeviceId.ToGuid();
        }

        private static string GetSyncLogQuestionnaireId(Guid questionnaireId, long questionnaireVersion, string syncItemType)
        {
            return string.Concat(new QuestionnaireIdentity(questionnaireId, questionnaireVersion).ToString(), "$", syncItemType);
        }
    }
}