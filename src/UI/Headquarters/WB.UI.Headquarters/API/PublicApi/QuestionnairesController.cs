using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.API.PublicApi
{
    [RoutePrefix("api/v1/questionnaires")]
    public class QuestionnairesController : BaseApiServiceController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItems;
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly ISerializer serializer;
        protected readonly IQuestionnaireStorage questionnaireStorage;

        public QuestionnairesController(ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAllInterviewsFactory allInterviewsViewFactory,
            ISerializer serializer,
            IQuestionnaireStorage questionnaireStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItems)
            : base(logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.serializer = serializer;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireBrowseItems = questionnaireBrowseItems;
        }

        /// <summary>
        /// Gets list of imported questionnaires 
        /// </summary>
        /// <param name="limit">Limit number of returned rows. Max allowed value is 40</param>
        /// <param name="offset">Skip rows</param>
        [HttpGet]
        [Route("")]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public QuestionnaireApiView Questionnaires(int limit = 10, int offset = 1)
        {
            var input = new QuestionnaireBrowseInputModel()
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
            };

            var questionnairesFromStore = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnairesFromStore);
        }

        [HttpGet]
        [Route("{id:guid}/{version:long?}")]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public QuestionnaireApiView Questionnaires(Guid id, long? version = null, int limit = 10, int offset = 1)
        {
            var input = new QuestionnaireBrowseInputModel()
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = id,
                Version = version
            };

            var questionnaires = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnaires);
        }

        /// <summary>
        /// Gets list of possible interview statuses 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("statuses")]
        [ResponseType(typeof(InterviewStatus))]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public IEnumerable<string> QuestionnairesStatuses()
        {
            return Enum.GetNames(typeof(InterviewStatus));
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/document")]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public HttpResponseMessage QuestionnaireDocument(Guid id, long version)
        {
            var questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocumentVersioned == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var questionnaireDocumentVersionedSerialized = this.serializer.Serialize(questionnaireDocumentVersioned);
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(questionnaireDocumentVersionedSerialized, Encoding.UTF8, "application/json");
            return response;
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/interviews")]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        public InterviewApiView Interviews(Guid id, long version, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = this.CheckAndRestrictOffset(offset),
                PageSize = this.CheckAndRestrictLimit(limit),
                QuestionnaireId = id,
                QuestionnaireVersion = version
            };

            var interviews = this.allInterviewsViewFactory.Load(input);

            return new InterviewApiView(interviews);
        }

        /// <summary>
        /// Sets audio recording enabled setting for provided questionnaire
        /// </summary>
        /// <param name="id">Questionnaire guid</param>
        /// <param name="version">Questionnaire version</param>
        /// <param name="requestData"></param>
        /// <response code="204">Questionnaire setting updated</response>
        /// <response code="404">Questionnaire cannot be found</response>
        [HttpPost]
        [Route("{id:guid}/{version:long}/recordAudio", Name = "RecordAudioSetting")]
        [ApiBasicAuth(UserRoles.ApiUser, UserRoles.Administrator, UserRoles.Headquarter, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
        [ObserverNotAllowedApi]
        public HttpResponseMessage RecordAudio(Guid id, long version, [FromBody]RecordAudioRequest requestData)
        {
            var questionnaire = 
                this.questionnaireBrowseItems.Query(_ => _.FirstOrDefault(
                    x => x.QuestionnaireId == id
                        && x.Version == version
                        && x.IsDeleted == false
                        && x.Disabled == false
            ));
            
            if (questionnaire == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            questionnaire.IsAudioRecordingEnabled = requestData.Enabled;

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
