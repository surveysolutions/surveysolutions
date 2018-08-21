using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.PublicApi
{
    [RoutePrefix("api/v1/questionnaires")]
    [ApiBasicAuth(new[] {UserRoles.ApiUser, UserRoles.Administrator}, TreatPasswordAsPlain = true, FallbackToCookieAuth = true)]
    public class QuestionnairesController : BaseApiServiceController
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly ISerializer serializer;
        protected readonly IQuestionnaireStorage questionnaireStorage;

        public QuestionnairesController(ILogger logger,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAllInterviewsFactory allInterviewsViewFactory,
            ISerializer serializer,
            IQuestionnaireStorage questionnaireStorage)
            : base(logger)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.serializer = serializer;
            this.questionnaireStorage = questionnaireStorage;
        }

        /// <summary>
        /// Gets list of imported questionnaires 
        /// </summary>
        /// <param name="limit">Limit number of returned rows. Max allowed value is 40</param>
        /// <param name="offset">Skip rows</param>
        [HttpGet]
        [Route("")]
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
        public IEnumerable<string> QuestionnairesStatuses()
        {
            return Enum.GetNames(typeof(InterviewStatus));
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/document")]
        public HttpResponseMessage QuestionnaireDocument(Guid id, long version)
        {
            var questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocumentVersioned == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var questionnaireDocumentVersionedSrialized = this.serializer.Serialize(questionnaireDocumentVersioned);
            var response = this.Request.CreateResponse(questionnaireDocumentVersionedSrialized);
            return response;
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/interviews")]
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
    }

}
