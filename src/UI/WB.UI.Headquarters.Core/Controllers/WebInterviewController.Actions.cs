using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
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

        public WebInterviewBinaryController(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ICommandService commandService,
            IImageProcessingService imageProcessingService, 
            IWebInterviewNotificationService webInterviewNotificationService, 
            IAudioFileStorage audioFileStorage, 
            IAudioProcessingService audioProcessingService, 
            IImageFileStorage imageFileStorage)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.commandService = commandService;
            this.imageProcessingService = imageProcessingService;
            this.webInterviewNotificationService = webInterviewNotificationService;
            this.audioFileStorage = audioFileStorage;
            this.audioProcessingService = audioProcessingService;
            this.imageFileStorage = imageFileStorage;
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

                var audioInfo = await this.audioProcessingService.CompressAudioFileAsync(bytes);

                var fileName = $@"{question.VariableName}__{questionIdentity.RosterVector}.m4a";

                audioFileStorage.StoreInterviewBinaryData(id, fileName, audioInfo.Binary, audioInfo.MimeType);

                var audioDuration = audioInfo.Duration == TimeSpan.Zero 
                    ? (Double.TryParse(duration, out var dur) ? TimeSpan.FromSeconds(dur) : TimeSpan.Zero)
                    : audioInfo.Duration;

                var command = new AnswerAudioQuestionCommand(interview.Id,
                    interview.CurrentResponsibleId, questionIdentity.Id, questionIdentity.RosterVector,
                    fileName, 
                    audioDuration);

                this.commandService.Execute(command);
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

            try
            {
                await using var ms = new MemoryStream();

                await file.CopyToAsync(ms);

                this.imageProcessingService.Validate(ms.ToArray());

                filename = AnswerUtils.GetPictureFileName(question.VariableName, questionIdentity.RosterVector);
                var responsibleId = interview.CurrentResponsibleId;

                this.imageFileStorage.StoreInterviewBinaryData(interview.Id, filename, ms.ToArray(), file.ContentType);

                this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                    responsibleId, questionIdentity.Id, questionIdentity.RosterVector, filename));
            }
            catch (Exception e)
            {
                if (filename != null)
                    await this.imageFileStorage.RemoveInterviewBinaryData(interview.Id, filename);

                webInterviewNotificationService.MarkAnswerAsNotSaved(id, questionIdentity, e);
                throw;
            }
            return this.Json("ok");
        }
    }
}
