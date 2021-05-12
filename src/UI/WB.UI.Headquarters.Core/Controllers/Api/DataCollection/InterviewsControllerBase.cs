using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    [RequestSizeLimit(1 * 1024 * 1024 * 1024)]
    public abstract class InterviewsControllerBase : ControllerBase
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IAuthorizedUser authorizedUser;
        protected readonly IInterviewPackagesService packagesService;
        protected readonly ICommandService commandService;
        protected readonly IMetaInfoBuilder metaBuilder;
        protected readonly IJsonAllTypesSerializer synchronizationSerializer;
        protected readonly IHeadquartersEventStore eventStore;
        protected readonly IInterviewInformationFactory interviewsFactory;

        protected InterviewsControllerBase(IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService packagesService,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer,
            IHeadquartersEventStore eventStore,
            IAudioAuditFileStorage audioAuditFileStorage,
            IWebHostEnvironment webHostEnvironment)
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
            this.webHostEnvironment = webHostEnvironment;
        }

        public virtual ActionResult<List<InterviewApiView>> Get()
        {
            var interviewApiViews = GetInProgressInterviewsForResponsible(this.authorizedUser.Id)
                .ToList();

            var isNeedUpdateApp = IsNeedUpdateApp(interviewApiViews);
            if (isNeedUpdateApp)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            return interviewApiViews.Select(interview => new InterviewApiView
            {
                Id = interview.Id,
                QuestionnaireIdentity = interview.QuestionnaireIdentity,
                IsRejected = interview.IsRejected,
                ResponsibleId = interview.ResponsibleId,
                Sequence = interview.LastEventSequence,
                LastEventId = interview.LastEventId,
                IsMarkedAsReceivedByInterviewer = interview.IsReceivedByInterviewer
            }).ToList();
        }

        private bool IsNeedUpdateApp(List<InterviewInformation> interviews)
        {
            var productVersion = this.Request.GetProductVersionFromUserAgent(ProductName);

            if (productVersion != null && productVersion < new Version(20, 5))
            {
                if (interviews.Any(interview => interviewsFactory.HasAnySmallSubstitutionEvent(interview.Id)))
                    return true;
            }

            if (productVersion != null && productVersion < new Version(21, 5))
            {
                if (interviews.Any(interview => interview.Mode != InterviewMode.Unknown))
                {
                    return true;
                }
            }

            return false;
        }


        protected abstract IEnumerable<InterviewInformation> GetInProgressInterviewsForResponsible(Guid responsibleId);

        public virtual IActionResult LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.authorizedUser.Id));
            return StatusCode(StatusCodes.Status204NoContent);
        }

        protected abstract string ProductName { get; }
        
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

            var interview = interviewsFactory.GetInterviewsByIds(new [] { id }).SingleOrDefault();

            return new InterviewUploadState
            {
                IsEventsUploaded = doesEventsExists,
                ImagesFilesNames = imageNames,
                AudioFilesNames = audioNames,
                ImageQuestionsFilesMd5 = imagesQuestionsMd5,
                AudioQuestionsFilesMd5 = audioQuestionsFilesMd5,
                AudioAuditFilesMd5 = audioAuditFilesMd5,
                ResponsibleId = interview?.ResponsibleId,
                IsReceivedByInterviewer = interview?.IsReceivedByInterviewer ?? false,
                Mode = interview?.Mode ?? InterviewMode.Unknown
            };
        }

        private static async Task<HashSet<string>> GetMd5Caches(List<InterviewBinaryDataDescriptor> descriptors)
        {
            List<string> caches = new List<string>(descriptors.Count);

            foreach (var descriptor in descriptors)
            {
                var md5 = await GetMd5Cache(descriptor);
                if (md5 != null)
                    caches.Add(md5);
            }

            return caches.ToHashSet();
        }

        private static async Task<string> GetMd5Cache(InterviewBinaryDataDescriptor descriptor)
        {
            var fileContent = await descriptor.GetData();
            if (fileContent == null)
                return null;

            using var crypto = MD5.Create();
            var hash = crypto.ComputeHash(fileContent);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            return hashString;
        }

        protected IActionResult DetailsV3(Guid id)
        {
            var allEvents = eventStore.Read(id, 0).ToList();

            var isNeedUpdateApp = IsNeedUpdateApp(allEvents);

            if (isNeedUpdateApp)
                return StatusCode(StatusCodes.Status426UpgradeRequired);

            return new JsonResult(allEvents, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
        }

        private bool IsNeedUpdateApp(List<CommittedEvent> allEvents)
        {
            if (webHostEnvironment.IsDevelopment())
                return false;

            var productVersion = this.Request.GetProductVersionFromUserAgent(ProductName);
            if (productVersion != null && productVersion >= new Version(20, 5))
                return false;

            return allEvents.Any(e =>
            {
                if (e.Payload is SubstitutionTitlesChanged titlesChanged)
                {
                    return titlesChanged.Questions.Length == 0 && titlesChanged.Groups.Length == 0 && titlesChanged.StaticTexts.Length == 0;
                }

                return false;
            });
        }
        
        protected IActionResult DetailsAfterV3(Guid id, Guid eventId)
        {
            var events = eventStore.ReadAfter(id, eventId).ToList();
            return new JsonResult(events, Infrastructure.Native.Storage.EventSerializerSettings.SyncronizationJsonSerializerSettings);
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
