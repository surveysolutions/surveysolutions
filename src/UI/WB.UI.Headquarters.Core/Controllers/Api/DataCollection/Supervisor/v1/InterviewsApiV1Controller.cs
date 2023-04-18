using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1/interviews")]
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
            IHeadquartersEventStore eventStore,
            IWebHostEnvironment webHostEnvironment) :
            base(imageFileStorage,
                audioFileStorage,
                authorizedUser,
                interviewsFactory,
                packagesService,
                commandService,
                metaBuilder,
                synchronizationSerializer,
                eventStore,
                audioAuditFileStorage,
                webHostEnvironment)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        [Route("")]
        public override ActionResult<List<InterviewApiView>> Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        [Route("{id:guid}")]
        public IActionResult Details(Guid id) => base.DetailsV3(id);

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        [Route("{id:guid}")]
        public ActionResult<InterviewUploadResult> Post([FromBody] InterviewPackageApiView package) => base.PostV3(package);

        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        [HttpPost]
        [Route("{id:guid}/logstate")]

        public override IActionResult LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        [Route("{id:guid}/image")]
        public override IActionResult PostImage([FromBody] PostFileRequest request) => base.PostImage(request);

        [HttpPost]
        [Route("{id:guid}/audio")]
        public override IActionResult PostAudio([FromBody] PostFileRequest request) => base.PostAudio(request);

        [HttpPost]
        [Route("{id:guid}/audioaudit")]
        public override IActionResult PostAudioAudit([FromBody] PostFileRequest request) => base.PostAudioAudit(request);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.CheckIsPackageDuplicated)]
        [Route("{id:guid}/getInterviewUploadState")]
        public Task<InterviewUploadState> GetInterviewUploadState(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
            => base.GetInterviewUploadStateImpl(id, eventStreamSignatureTag);
    }
}
