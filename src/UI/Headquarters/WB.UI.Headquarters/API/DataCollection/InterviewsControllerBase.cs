using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class InterviewsControllerBase : ApiController
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        protected readonly IAuthorizedUser authorizedUser;
        protected readonly IInterviewPackagesService packagesService;
        protected readonly ICommandService commandService;
        protected readonly IMetaInfoBuilder metaBuilder;
        protected readonly IJsonAllTypesSerializer synchronizationSerializer;
        protected readonly IHeadquartersEventStore eventStore;
        protected readonly IInterviewInformationFactory interviewsFactory;

        protected InterviewsControllerBase(
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService packagesService,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer,
            IHeadquartersEventStore eventStore,
            IAudioAuditFileStorage audioAuditFileStorage)
        {
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
            this.authorizedUser = authorizedUser;
            this.interviewsFactory = interviewsFactory;
            this.packagesService = packagesService;
            this.commandService = commandService;
            this.metaBuilder = metaBuilder;
            this.synchronizationSerializer = synchronizationSerializer;
            this.eventStore = eventStore;
            this.audioAuditFileStorage = audioAuditFileStorage;
        }

        public virtual HttpResponseMessage Get()
        {
            var resultValue = GetInProgressInterviewsForResponsible(this.authorizedUser.Id)
                .Select(interview => new InterviewApiView
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected,
                    ResponsibleId = interview.ResponsibleId,
                    Sequence = interview.LastEventSequence,
                    IsMarkedAsReceivedByInterviewer = interview.IsReceivedByInterviewer
                }).ToList();

            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = false,
                NoCache = true
            };

            return response;
        }

        protected abstract IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid responsibleId);

        public abstract HttpResponseMessage LogInterviewAsSuccessfullyHandled(Guid interviewId);
        
        public virtual HttpResponseMessage PostImage(PostFileRequest request)
        {
            this.imageFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), null);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        public virtual HttpResponseMessage PostAudio(PostFileRequest request)
        {
            this.audioFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }

        public virtual HttpResponseMessage PostAudioAudit(PostFileRequest request)
        {
            this.audioAuditFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        protected InterviewUploadState GetInterviewUploadStateImpl(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
        {
            var doesEventsExists = this.packagesService.IsPackageDuplicated(eventStreamSignatureTag);

            // KP-12038 media files are not updated if interviewer changes them after reject
            var imageNames = new HashSet<string>(); //this.imageFileStorage.GetBinaryFilesForInterview(id).Select(bf => bf.FileName).ToHashSet();
            var audioNames = new HashSet<string>(); //this.audioFileStorage.GetBinaryFilesForInterview(id).Select(bf => bf.FileName).ToHashSet();

            return new InterviewUploadState
            {
                IsEventsUploaded = doesEventsExists,
                ImagesFilesNames = imageNames,
                AudioFilesNames = audioNames
            };
        }

        protected JsonResult<List<CommittedEvent>> DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return Json(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        protected HttpResponseMessage PostV3(InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server cannot accept empty package content.");

            var targetStatus = (InterviewStatus) package.MetaInfo.Status;
            var interviewPackage = new InterviewPackage
            {
                InterviewId = package.InterviewId,
                QuestionnaireId = package.MetaInfo.TemplateId,
                QuestionnaireVersion = package.MetaInfo.TemplateVersion,
                InterviewStatus = targetStatus,
                ResponsibleId =  package.MetaInfo.ResponsibleId,
                IsCensusInterview = package.MetaInfo.CreatedOnClient ?? false,
                IncomingDate = DateTime.UtcNow,
                Events = package.Events
            };

            this.packagesService.StoreOrProcessPackage(interviewPackage);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
