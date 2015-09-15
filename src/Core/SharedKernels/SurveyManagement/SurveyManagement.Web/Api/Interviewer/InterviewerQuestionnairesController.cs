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

            return this.questionnaireBrowseViewFactory.Load(query).Items.Where(questionnaire => questionnaire.AllowCensusMode)
                .Select(questionnaire => new QuestionnaireIdentity(questionnaire.QuestionnaireId, questionnaire.Version)).ToList();
        }

        [HttpGet]
        [Route("{id:guid}/{version:int}")]
        public QuestionnaireApiView Get(Guid id, int version)
        {
            return new QuestionnaireApiView()
            {
                QuestionnaireDocument = this.jsonUtils.Serialize(this.questionnaireStore.AsVersioned().Get(id.FormatGuid(), version).Questionnaire),
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

            return response;
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/logstate")]
        public void LogQuestionnaireAsSuccessfullyHandled(Guid id, int version)
        {
            this.syncLogger.TrackPackageRequest(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, SyncItemType.Questionnaire,
                new QuestionnaireIdentity(id, version).ToString());
        }

        [HttpPost]
        [Route("{id:guid}/{version:int}/assembly/logstate")]
        public void LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version)
        {
            this.syncLogger.TrackPackageRequest(this.GetInterviewerDeviceId(),
                this.globalInfoProvider.GetCurrentUser().Id, SyncItemType.QuestionnaireAssembly,
                new QuestionnaireIdentity(id, version).ToString());
        }

        private Guid GetInterviewerDeviceId()
        {
            return this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null)).DeviceId.ToGuid();
        }
    }
}