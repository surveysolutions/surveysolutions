using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class InterviewsControllerBase : ControllerBase
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
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

        public virtual ActionResult<List<InterviewApiView>> Get()
        {
            List<InterviewApiView> resultValue = GetInProgressInterviewsForResponsible(this.authorizedUser.Id)
                .Select(interview => new InterviewApiView
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected,
                    ResponsibleId = interview.ResponsibleId,
                    Sequence = interview.LastEventSequence,
                    IsMarkedAsReceivedByInterviewer = interview.IsReceivedByInterviewer
                }).ToList();

            var response = resultValue;

            return response;
        }

        protected abstract IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid responsibleId);

        public virtual IActionResult LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.authorizedUser.Id));
            return StatusCode(StatusCodes.Status204NoContent);
        }
        
        public virtual IActionResult PostImage([FromBody] PostFileRequest request)
        {
            this.imageFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), null);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        public virtual IActionResult PostAudio([FromBody] PostFileRequest request)
        {
            this.audioFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);
            return StatusCode(StatusCodes.Status204NoContent);
        }

        public virtual IActionResult PostAudioAudit([FromBody] PostFileRequest request)
        {
            this.audioAuditFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data), request.ContentType);

            return StatusCode(StatusCodes.Status204NoContent);
        }

        protected async Task<InterviewUploadState> GetInterviewUploadStateImpl(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
        {
            var doesEventsExists = this.packagesService.IsPackageDuplicated(eventStreamSignatureTag);

            // KP-12038 media files are not updated if interviewer changes them after reject
            var imageNames = new HashSet<string>(); 
            var audioNames = new HashSet<string>();

            var imagesQuestionsMd5 = (await GetMd5Caches(await this.imageFileStorage.GetBinaryFilesForInterview(id)));
            var audioQuestionsFilesMd5 = (await GetMd5Caches(await this.audioFileStorage.GetBinaryFilesForInterview(id)));
            var audioAuditFilesMd5 = (await GetMd5Caches(await this.audioAuditFileStorage.GetBinaryFilesForInterview(id)));

            return new InterviewUploadState
            {
                IsEventsUploaded = doesEventsExists,
                ImagesFilesNames = imageNames,
                AudioFilesNames = audioNames,
                ImageQuestionsFilesMd5 = imagesQuestionsMd5,
                AudioQuestionsFilesMd5 = audioQuestionsFilesMd5,
                AudioAuditFilesMd5 = audioAuditFilesMd5,
            };
        }

        private static async Task<HashSet<string>> GetMd5Caches(List<InterviewBinaryDataDescriptor> descriptors)
        {
            List<string> caches = new List<string>(descriptors.Count);

            foreach (var descriptor in descriptors)
            {
                var md5 = await GetMd5Cache(descriptor);
                caches.Add(md5);
            }

            return caches.ToHashSet();
        }

        private static async Task<string> GetMd5Cache(InterviewBinaryDataDescriptor descriptor)
        {
            var fileContent = await descriptor.GetData();
            using var crypto = MD5.Create();
            var hash = crypto.ComputeHash(fileContent);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return hashString;
        }

        protected IActionResult DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();
            return new JsonResult(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        protected IActionResult PostV3(InterviewPackageApiView package)
        {
            if (string.IsNullOrEmpty(package.Events))
                return BadRequest("Server cannot accept empty package content.");

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

            return Ok();
        }
    }
}
