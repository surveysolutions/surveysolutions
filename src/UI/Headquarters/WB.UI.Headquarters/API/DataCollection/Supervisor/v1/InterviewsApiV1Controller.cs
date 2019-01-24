using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewsApiV1Controller : SupervisorInterviewsControllerBase
    {
        public InterviewsApiV1Controller(IImageFileStorage imageFileStorage, 
            IAudioFileStorage audioFileStorage,
            IAudioAuditFileStorage audioAuditFileStorage,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService packagesService,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer,
            IHeadquartersEventStore eventStore) : 
            base(imageFileStorage,
                audioFileStorage,
                authorizedUser,
                interviewsFactory,
                packagesService,
                commandService,
                metaBuilder,
                synchronizationSerializer, 
                eventStore, 
                audioAuditFileStorage)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        public JsonResult<List<CommittedEvent>> Details(Guid id) => base.DetailsV3(id);

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        public HttpResponseMessage Post(InterviewPackageApiView package) => base.PostV3(package);

        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        [HttpPost]
        public override HttpResponseMessage LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        public override HttpResponseMessage PostImage(PostFileRequest request) => base.PostImage(request);

        [HttpPost]
        public override HttpResponseMessage PostAudio(PostFileRequest request) => base.PostAudio(request);

        [HttpPost]
        public override HttpResponseMessage PostAudioAudit(PostFileRequest request) => base.PostAudioAudit(request);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.CheckIsPackageDuplicated)]
        public InterviewUploadState GetInterviewUploadState(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
            => base.GetInterviewUploadStateImpl(id, eventStreamSignatureTag);
    }
}
