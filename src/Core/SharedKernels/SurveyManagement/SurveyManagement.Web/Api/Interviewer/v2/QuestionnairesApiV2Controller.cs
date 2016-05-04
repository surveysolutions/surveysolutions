using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    public class QuestionnairesApiV2Controller : QuestionnairesControllerBase
    {
        public QuestionnairesApiV2Controller(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            ISerializer serializer,
            IPlainQuestionnaireRepository plainQuestionnaireRepository,
            IPlainStorageAccessor<QuestionnaireBrowseItem> readsideRepositoryWriter) : base(
                plainQuestionnaireRepository: plainQuestionnaireRepository,
                readsideRepositoryWriter: readsideRepositoryWriter,
                questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor,
                questionnaireBrowseViewFactory: questionnaireBrowseViewFactory,
                serializer: serializer)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Census() => base.Census();
        [HttpGet]
        public  HttpResponseMessage Get(Guid id, int version, long contentVersion) => base.Get(id, version, contentVersion, false);
        [HttpGet]
        public override HttpResponseMessage GetAssembly(Guid id, int version) => base.GetAssembly(id, version);
        [HttpPost]
        public override void LogQuestionnaireAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAsSuccessfullyHandled(id, version);
        [HttpPost]
        public override void LogQuestionnaireAssemblyAsSuccessfullyHandled(Guid id, int version) => base.LogQuestionnaireAssemblyAsSuccessfullyHandled(id, version);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetQuestionnaireAttachments)]
        public HttpResponseMessage GetAttachments(Guid id, int version)
        {
            var questionnaireDocument = this.plainQuestionnaireRepository.GetQuestionnaireDocument(id, version);

            if (questionnaireDocument == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var attachmentIds = questionnaireDocument.Attachments.Select(a => a.ContentId).ToList();

            var response = this.Request.CreateResponse(attachmentIds);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}