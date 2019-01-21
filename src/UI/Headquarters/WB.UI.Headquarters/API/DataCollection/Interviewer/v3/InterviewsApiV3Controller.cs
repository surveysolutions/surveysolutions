﻿using System;
using System.Collections.Generic;
using System.Net;
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
using WB.Core.SharedKernels.DataCollection.Events;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v3
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class InterviewsApiV3Controller : InterviewerInterviewsControllerBase
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;

        public InterviewsApiV3Controller(
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService packagesService,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer,
            IHeadquartersEventStore eventStore) :
            base(imageFileStorage,
                audioFileStorage, authorizedUser, interviewsFactory, packagesService, commandService, metaBuilder,
                synchronizationSerializer, eventStore)
        {
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public override HttpResponseMessage Get() => base.Get();

        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        [HttpPost]
        public override HttpResponseMessage LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        public JsonResult<List<CommittedEvent>> Details(Guid id) => base.DetailsV3(id);

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        public HttpResponseMessage Post(InterviewPackageApiView package) => base.PostV3(package);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.CheckObsoleteInterviews)]
        public HttpResponseMessage CheckObsoleteInterviews(List<ObsoletePackageCheck> knownPackages)
        {
            List<Guid> obsoleteInterviews = new List<Guid>();
            foreach (var obsoletePackageCheck in knownPackages)
            {
                if (this.eventStore.HasEventsAfterSpecifiedSequenceWithAnyOfSpecifiedTypes(
                    obsoletePackageCheck.SequenceOfLastReceivedEvent,
                    obsoletePackageCheck.InterviewId, EventsThatChangeAnswersStateProvider.GetTypeNames()))
                {
                    obsoleteInterviews.Add(obsoletePackageCheck.InterviewId);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, obsoleteInterviews);
        }

        [HttpPost]
        public override HttpResponseMessage PostImage(PostFileRequest request) => base.PostImage(request);
        [HttpPost]
        public override HttpResponseMessage PostAudio(PostFileRequest request) => base.PostAudio(request);

        [HttpPost]
        public HttpResponseMessage PostAudioAudit(PostFileRequest request)
        {
            this.audioFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.CheckIsPackageDuplicated)]
        public InterviewUploadState GetInterviewUploadState(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
            => base.GetInterviewUploadStateImpl(id, eventStreamSignatureTag);
    }
}
