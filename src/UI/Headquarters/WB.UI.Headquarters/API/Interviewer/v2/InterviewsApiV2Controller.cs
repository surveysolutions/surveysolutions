using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class InterviewsApiV2Controller : InterviewsControllerBase
    {
        public InterviewsApiV2Controller(
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService incomingSyncPackagesQueue,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer) : base(
                imageFileStorage: imageFileStorage,
                audioFileStorage: audioFileStorage,
                authorizedUser: authorizedUser,
                interviewsFactory: interviewsFactory,
                interviewPackagesService: incomingSyncPackagesQueue,
                commandService: commandService,
                metaBuilder: metaBuilder,
                synchronizationSerializer: synchronizationSerializer)
        {
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterview)]
        public HttpResponseMessage Details(Guid id)
        {
            var interviewDetails = this.interviewsFactory.GetInProgressInterviewDetails(id);
            if (interviewDetails == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var interviewMetaInfo = this.metaBuilder.GetInterviewMetaInfo(interviewDetails);

            var response = this.Request.CreateResponse(new InterviewerInterviewApiView
            {
                AnswersOnPrefilledQuestions = interviewMetaInfo?.FeaturedQuestionsMeta
                    .Select(prefilledQuestion => new AnsweredQuestionSynchronizationDto(prefilledQuestion.PublicKey, 
                                                                                        new decimal[0], prefilledQuestion.Value, 
                                                                                        new CommentSynchronizationDto[0]))
                    .ToArray(),
                Details = this.synchronizationSerializer.Serialize(interviewDetails)
            });

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }
        [HttpPost]
        public override void LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.PostInterview)]
        public IHttpActionResult Post(InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return this.BadRequest("Server cannot accept empty package content.");

            var interviewPackage = new InterviewPackage
            {
                InterviewId = package.InterviewId,
                QuestionnaireId = package.MetaInfo.TemplateId,
                QuestionnaireVersion = package.MetaInfo.TemplateVersion,
                InterviewStatus = (InterviewStatus)package.MetaInfo.Status,
                ResponsibleId = package.MetaInfo.ResponsibleId,
                IsCensusInterview = package.MetaInfo.CreatedOnClient ?? false,
                IncomingDate = DateTime.UtcNow,
                Events = package.Events
            };

            this.interviewPackagesService.StoreOrProcessPackage(interviewPackage);

            return this.Ok ();
        }
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);
        [HttpPost]
        public override void PostAudio(PostFileRequest request) => base.PostAudio(request);
    }
}