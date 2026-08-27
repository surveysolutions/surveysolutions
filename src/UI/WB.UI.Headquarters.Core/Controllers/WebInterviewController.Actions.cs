using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Headquarters.Filters;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers
{
    [TypeFilter(typeof(WebInterviewErrorFilterAttribute))]
    [WebInterviewAuthorize]
    public class WebInterviewBinaryController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ICommandService commandService;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IInterviewBinaryCleanupService interviewBinaryCleanupService;
        private readonly IAggregateLock aggregateLock;

        public WebInterviewBinaryController(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandService commandService,
            IImageProcessingService imageProcessingService, 
            IWebInterviewNotificationService webInterviewNotificationService, 
            IAudioFileStorage audioFileStorage, 
            IAudioProcessingService audioProcessingService, 
            IImageFileStorage imageFileStorage,
            IInterviewBinaryCleanupService interviewBinaryCleanupService,
            IAggregateLock aggregateLock)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.imageProcessingService = imageProcessingService;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.audioFileStorage = audioFileStorage;
            this.audioProcessingService = audioProcessingService;
            this.imageFileStorage = imageFileStorage;
            this.interviewBinaryCleanupService = interviewBinaryCleanupService;
            this.aggregateLock = aggregateLock;
        }

        [HttpPost]
        public async Task<ActionResult> Audio(Guid id, [FromForm] string questionId, [FromForm] string duration, [FromForm] IFormFile file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(id.FormatGuid());

            var questionIdentity = Identity.Parse(questionId);
            InterviewTreeQuestion question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question.IsAudio)
            {
                return this.Json("fail");
            }
            try
            {
                await using var ms = new MemoryStream();

                await file.CopyToAsync(ms);
                byte[] bytes = ms.ToArray();

                string contentType = file.ContentType;
                
                var fileName = $@"{question.VariableName}__{questionIdentity.RosterVector}.aac";
                
                var audioDuration = TimeSpan.Zero;
                if(contentType is "audio/wav" or "audio/x-wav")
                {
                    var audioInfo = await this.audioProcessingService.CompressAudioFileAsync(bytes, contentType);
                    bytes = audioInfo.Binary;
                    contentType = audioInfo.MimeType;
                    audioDuration = audioInfo.Duration == TimeSpan.Zero 
                        ? (Double.TryParse(duration, out var dur) ? TimeSpan.FromSeconds(dur) : TimeSpan.Zero)
                        : audioInfo.Duration;
                }
                else
                {
                    audioDuration = (Double.TryParse(duration, out var dur)
                        ? TimeSpan.FromSeconds(dur)
                        : TimeSpan.Zero);
                }

                this.aggregateLock.RunWithLock(id.FormatGuid(), () =>
                {
                    var lockedInterview = this.statefulInterviewRepository.Get(id.FormatGuid());
                    audioFileStorage.StoreInterviewBinaryData(id, fileName, bytes, contentType);
                    var command = new AnswerAudioQuestionCommand(lockedInterview.Id,
                        lockedInterview.CurrentResponsibleId, questionIdentity.Id, questionIdentity.RosterVector,
                        fileName, 
                        audioDuration);

                    this.commandService.Execute(command);
                });
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(id, questionIdentity, e);
                throw;
            }
            return this.Json("ok");
        }

        [HttpPost]
        public async Task<ActionResult> Image(Guid id, [FromForm] string questionId, [FromForm] IFormFile file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(id.FormatGuid());

            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question.IsMultimedia)
            {
                return this.Json("fail");
            }

            string filename = null;
            string previousFilename = null;
            byte[] previousFileContent = null;
            string previousFileContentType = null;
            bool answerCommandSucceeded = false;

            try
            {
                await using var ms = new MemoryStream();

                await file.CopyToAsync(ms);

                this.imageProcessingService.Validate(ms.ToArray());

                var bytes = ms.ToArray();
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var generatedFilename = AnswerUtils.GetPictureFileName(question.VariableName, questionIdentity.RosterVector, extension);

                this.aggregateLock.RunWithLock(interview.Id.FormatGuid(), () =>
                {
                    var lockedInterview = this.statefulInterviewRepository.Get(id.FormatGuid());
                    var lockedQuestion = lockedInterview.GetQuestion(questionIdentity);
                    var responsibleId = lockedInterview.CurrentResponsibleId;

                    if (lockedQuestion.IsAnswered())
                        previousFilename = lockedQuestion.GetAsInterviewTreeMultimediaQuestion().GetAnswer()?.FileName;

                    filename = previousFilename != null &&
                               string.Equals(previousFilename, generatedFilename, StringComparison.OrdinalIgnoreCase)
                        ? previousFilename
                        : generatedFilename;

                    if (previousFilename != null && string.Equals(previousFilename, filename, StringComparison.Ordinal))
                    {
                        previousFileContent = this.imageFileStorage.GetInterviewBinaryData(lockedInterview.Id, previousFilename);
                        previousFileContentType = ContentTypeHelper.GetImageContentType(previousFilename);
                    }

                    this.imageFileStorage.StoreInterviewBinaryData(lockedInterview.Id, filename, bytes, file.ContentType);

                    this.commandService.Execute(new AnswerPictureQuestionCommand(lockedInterview.Id,
                        responsibleId, questionIdentity.Id, questionIdentity.RosterVector, filename));

                    answerCommandSucceeded = true;

                    if (previousFilename != null && !string.Equals(previousFilename, filename, StringComparison.Ordinal))
                    {
                        try
                        {
                            this.imageFileStorage.RemoveInterviewBinaryData(lockedInterview.Id, previousFilename).GetAwaiter().GetResult();
                        }
                        catch (Exception e)
                        {
                            this.interviewBinaryCleanupService.EnqueueImageCleanup(lockedInterview.Id, previousFilename, e);
                        }
                    }

                    this.interviewBinaryCleanupService.ProcessPending(lockedInterview.Id);
                });
            }
            catch (Exception e)
            {
                if (filename != null && !answerCommandSucceeded)
                {
                    if (previousFileContent != null && string.Equals(previousFilename, filename, StringComparison.Ordinal))
                        this.imageFileStorage.StoreInterviewBinaryData(interview.Id, previousFilename, previousFileContent, previousFileContentType);
                    else
                        await this.imageFileStorage.RemoveInterviewBinaryData(interview.Id, filename);
                }

                webInterviewNotificationService.MarkAnswerAsNotSaved(id, questionIdentity, e);
                throw;
            }

            return this.Json("ok");
        }
    }
}
