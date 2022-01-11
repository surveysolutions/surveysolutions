#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    [Route("api/v1/questionnaires")]
    [PublicApiJson]
    public class QuestionnairesPublicApiController : ControllerBase
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItems;
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly ISerializer serializer;
        protected readonly IQuestionnaireStorage questionnaireStorage;

        public QuestionnairesPublicApiController(IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAllInterviewsFactory allInterviewsViewFactory,
            ISerializer serializer,
            IQuestionnaireStorage questionnaireStorage, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItems)
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
        /// <param name="offset">Page number starting from 1. Actual skipped rows are calculated as `(offset - 1) * limit`</param>
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Obsolete("Use /graphql endpoint instead")]
        public QuestionnaireApiView Questionnaires(int limit = 10, int offset = 1 /* in v2 rename to page number or use as real offset */)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = offset.CheckAndRestrictOffset(),
                PageSize = limit.CheckAndRestrictLimit(),
            };

            var questionnairesFromStore = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnairesFromStore);
        }

        /// <summary>
        /// Gets list of imported questionnaires by questionnaire id
        /// </summary>
        /// <param name="id">Questionnaire Id</param>
        /// <param name="limit">Limit number of returned rows. Max allowed value is 40</param>
        /// <param name="offset">Page number starting from 1. Actual skipped rows are calculated as `(offset - 1) * limit`</param>
        [HttpGet]
        [Route("{id:guid}")]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Obsolete("Use /graphql endpoint instead")]
        public QuestionnaireApiView Questionnaires([FromRoute] Guid id, int limit = 10, int offset = 1)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = offset.CheckAndRestrictOffset(),
                PageSize = limit.CheckAndRestrictLimit(),
                QuestionnaireId = id
            };

            var questionnaires = this.questionnaireBrowseViewFactory.Load(input);

            return new QuestionnaireApiView(questionnaires);
        }

        /// <summary>
        /// Gets imported questionnaire by id and version
        /// </summary>
        /// <param name="version">Questionnaire version</param>
        /// <param name="id">Questionnaire Id</param>
        [HttpGet]
        [Route("{id:guid}/{version}")]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Obsolete("Use /graphql endpoint instead")]
        public ActionResult<QuestionnaireApiItem> Questionnaires([FromRoute] Guid id, [FromRoute] long version)
        {
            var input = new QuestionnaireBrowseInputModel
            {
                Page = 1,
                PageSize = 1,
                QuestionnaireId = id,
                Version = version
            };

            var questionnaires = this.questionnaireBrowseViewFactory.Load(input);

            var apiView = new QuestionnaireApiView(questionnaires);

            var result = apiView.Questionnaires.FirstOrDefault();

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        /// <summary>
        /// Gets list of possible interview statuses 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("statuses")]
        [ProducesResponseType(typeof(InterviewStatus), 200)]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Obsolete("Use /graphql endpoint instead")]
        public ActionResult<IEnumerable<string>> QuestionnairesStatuses()
        {
            return Enum.GetNames(typeof(InterviewStatus));
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/document")]
        [Produces(MediaTypeNames.Application.Json)]
        [Authorize(Roles = "ApiUser, Administrator")]
        public ActionResult QuestionnaireDocument(Guid id, long version)
        {
            var questionnaireDocumentVersioned = this.questionnaireStorage.GetQuestionnaireDocument(id, version);

            if (questionnaireDocumentVersioned == null)
            {
                return NotFound();
            }

            var questionnaireDocumentVersionedSerialized = this.serializer.Serialize(questionnaireDocumentVersioned);
            return Content(questionnaireDocumentVersionedSerialized, "application/json", Encoding.UTF8);
        }

        [HttpGet]
        [Route("{id:guid}/{version:long}/interviews")]
        [Authorize(Roles = "ApiUser, Administrator")]
        [Obsolete("Use /graphql endpoint instead")]
        public InterviewApiView Interviews(Guid id, long version, int limit = 10, int offset = 1)
        {
            var input = new AllInterviewsInputModel
            {
                Page = offset.CheckAndRestrictOffset(),
                PageSize = limit.CheckAndRestrictLimit(),
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
        [Route("{id:guid}/{version:long}/recordAudio", Name = "SetRecordAudioSetting")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Authorize(Roles = "ApiUser, Administrator, Headquarter")]
        [ObservingNotAllowed]
        public ActionResult RecordAudio(Guid id, long version, [FromBody, BindRequired]RecordAudioRequest requestData)
        {
            if (!ModelState.IsValid)
                return StatusCode(StatusCodes.Status400BadRequest, 
                    $@"Invalid parameter or property: {string.Join(',',ModelState.Keys.ToList())}");
            
            var questionnaire = 
                this.questionnaireBrowseItems.Query(_ => _.FirstOrDefault(
                    x => x.QuestionnaireId == id
                        && x.Version == version
                        && x.IsDeleted == false
                        && x.Disabled == false
            ));
            
            if (questionnaire == null)
            {
                return NotFound();
            }

            questionnaire.IsAudioRecordingEnabled = requestData.Enabled;

            return NoContent();
        }

        /// <summary>
        /// Gets audio recording enabled setting for provided questionnaire
        /// </summary>
        /// <param name="id">Questionnaire guid</param>
        /// <param name="version">Questionnaire version</param>
        /// <response code="404">Questionnaire cannot be found</response>
        [HttpGet]
        [Route("{id:guid}/{version:long}/recordAudio", Name = "GetRecordAudioSetting")]
        [Produces(MediaTypeNames.Application.Json)]
        [Authorize(Roles = "ApiUser, Administrator, Headquarter")]
        [ObservingNotAllowed]
        public ActionResult<AudioRecordingEnabled> GetRecordAudio(Guid id, long version)
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
                return NotFound();
            }

            return new JsonResult(new AudioRecordingEnabled
            {
               Enabled = questionnaire.IsAudioRecordingEnabled
            });
        }
    }
}
