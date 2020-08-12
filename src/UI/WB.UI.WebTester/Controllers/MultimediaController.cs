﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.UI.Shared.Web.Services;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    public class MultimediaController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IWebInterviewNotificationService webInterviewNotificationService;
        private readonly ICacheStorage<MultimediaFile,string> mediaStorage;
        private readonly IAudioProcessingService audioProcessingService;
        private readonly IImageProcessingService imageProcessingService;

        public MultimediaController(ICommandService commandService,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            ICacheStorage<MultimediaFile, string> mediaStorage,
            IAudioProcessingService audioProcessingService,
            IImageProcessingService imageProcessingService)
        {
            this.commandService = commandService;
            this.statefulInterviewRepository = statefulInterviewRepository ??
                                               throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.webInterviewNotificationService = webInterviewNotificationService ??
                                                   throw new ArgumentNullException(
                                                       nameof(webInterviewNotificationService));
            this.mediaStorage = mediaStorage;
            this.audioProcessingService = audioProcessingService;
            this.imageProcessingService = imageProcessingService;
        }

        public IActionResult AudioRecord(string interviewId, string fileName)
        {
            if (!Guid.TryParse(interviewId, out var id))
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            MultimediaFile? file = null;
            if (fileName != null)
            {
                file = this.mediaStorage.Get(fileName, id);
            }

            if (file == null || file.Data.Length == 0)
                return StatusCode(StatusCodes.Status404NotFound);

            return this.File(file.Data, file.MimeType, fileName);
        }

        [HttpPost]
        public async Task<ActionResult> Audio(string id, [FromForm] string questionId, [FromForm] IFormFile file)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

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

                var audioFile = await audioProcessingService.CompressAudioFileAsync(bytes);

                var fileName = $@"{question.VariableName}__{questionIdentity.RosterVector}.m4a";

                var entity = new  MultimediaFile(fileName, audioFile.Binary, audioFile.Duration, audioFile.MimeType);
                mediaStorage.Store(entity, fileName, interview.Id);

                var command = new AnswerAudioQuestionCommand(interview.Id,
                    interview.CurrentResponsibleId, questionIdentity.Id, questionIdentity.RosterVector,
                    fileName, audioFile.Duration);

                this.commandService.Execute(command);
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(Guid.Parse(id), questionIdentity, e);
                //webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId, WebInterview.GetUiMessageFromException(e));
                throw;
            }

            return this.Json("ok");
        }

        [HttpPost]
        public async Task<ActionResult> Image(string id, [FromForm]  string questionId, [FromForm]  IFormFile file)
        {
            var interview = this.statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question.IsMultimedia)
            {
                return this.Json("fail");
            }

            string? fileName = null;

            try
            {
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);

                var fileContent = ms.ToArray();
                this.imageProcessingService.Validate(fileContent);

                fileName = GetPictureFileName(question.VariableName, questionIdentity.RosterVector);

                var responsibleId = interview.CurrentResponsibleId;

                var entity = new MultimediaFile(fileName, fileContent, null,file.ContentType);
                this.mediaStorage.Store(entity, fileName, interview.Id);

                this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                    responsibleId, questionIdentity.Id, questionIdentity.RosterVector, fileName));
            }
            catch (Exception e)
            {
                if (fileName != null)
                    webInterviewNotificationService.MarkAnswerAsNotSaved(Guid.Parse(id), questionIdentity, e);
                throw;
            }

            return this.Json("ok");
        }

        private string GetPictureFileName(string variableName, RosterVector rosterVector) => AnswerUtils.GetPictureFileName(variableName, rosterVector);
    }
}
