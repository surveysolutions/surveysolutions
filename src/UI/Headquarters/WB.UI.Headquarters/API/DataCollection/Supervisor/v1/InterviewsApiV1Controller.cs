using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewsApiV1Controller : SupervisorInterviewsControllerBase
    {
        private readonly IHeadquartersEventStore eventStore;
        private readonly IInterviewPackagesService packagesService;

        public InterviewsApiV1Controller(IHeadquartersEventStore eventStore, IInterviewPackagesService packagesService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService interviewPackagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer) 
            : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, interviewPackagesService, commandService, metaBuilder, synchronizationSerializer)
        {
            this.eventStore = eventStore;
            this.packagesService = packagesService;
        }

        [HttpGet]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        public JsonResult<List<CommittedEvent>> Details(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return Json(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        public HttpResponseMessage Post(InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server cannot accept empty package content.");

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

            this.packagesService.StoreOrProcessPackage(interviewPackage);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public override void LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);
        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);
        [HttpPost]
        public override void PostAudio(PostFileRequest request) => base.PostAudio(request);

    }
}
