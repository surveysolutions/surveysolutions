using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles="Interviewer")]
    [Route("api/interviewer/v2/interviews")]
    public class InterviewsApiV2Controller : InterviewerInterviewsControllerBase
    {
        public InterviewsApiV2Controller(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAudioAuditFileStorage audioAuditFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService packagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore, IWebHostEnvironment webHostEnvironment) : 
            base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, packagesService, commandService, metaBuilder, synchronizationSerializer, eventStore, audioAuditFileStorage, webHostEnvironment)
        {
        }

        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public override ActionResult<List<InterviewApiView>> Get() => base.Get();

        [HttpPost]
        [Route("{id:guid}/logstate")]
        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        public override IActionResult LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        [Route("{id:guid}")]
        [WriteToSyncLog(SynchronizationLogType.PostInterview)]
        public IActionResult Post([FromBody]InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return BadRequest("Server cannot accept empty package content.");

            var interviewPackage = new InterviewPackage
            {
                InterviewId = package.InterviewId,
                QuestionnaireId = package.MetaInfo.TemplateId,
                QuestionnaireVersion = package.MetaInfo.TemplateVersion,
                InterviewStatus = (InterviewStatus)package.MetaInfo.Status,
                ResponsibleId = package.MetaInfo.ResponsibleId,
                IsCensusInterview = package.MetaInfo.CreatedOnClient ?? false,
                IncomingDate = DateTime.UtcNow,
                Events = package.Events,
                IsFullEventStream = package.IsFullEventStream
            };

            this.packagesService.StoreOrProcessPackage(interviewPackage);

            return Ok();
        }

        [HttpPost]
        [Route("{id:guid}/image")]
        public override IActionResult PostImage(PostFileRequest request) => base.PostImage(request);
        [HttpPost]
        [Route("{id:guid}/audio")]
        public override IActionResult PostAudio(PostFileRequest request) => base.PostAudio(request);
        [HttpPost]
        [Route("{id:guid}/audioaudit")]
        public override IActionResult PostAudioAudit(PostFileRequest request) => base.PostAudioAudit(request);
    }
}
