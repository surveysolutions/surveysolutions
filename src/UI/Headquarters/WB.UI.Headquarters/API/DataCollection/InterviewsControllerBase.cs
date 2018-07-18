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
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class InterviewsControllerBase : ApiController
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAuthorizedUser authorizedUser;
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
            IHeadquartersEventStore eventStore)
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
                    Sequence = interview.Sequence
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

        
        public virtual void LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.authorizedUser.Id));
        }
        
        public virtual void PostImage(PostFileRequest request)
        {
            this.imageFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), null);
        }

        public virtual void PostAudio(PostFileRequest request)
        {
            this.audioFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);
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
