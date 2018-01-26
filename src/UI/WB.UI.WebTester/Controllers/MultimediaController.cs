﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
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
        private readonly IImageProcessingService imageProcessingService;

        public MultimediaController(ICommandService commandService,
            IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            ICacheStorage<MultimediaFile, string> mediaStorage,
            IImageProcessingService imageProcessingService)
        {
            this.commandService = commandService;
            this.statefulInterviewRepository = statefulInterviewRepository ??
                                               throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.webInterviewNotificationService = webInterviewNotificationService ??
                                                   throw new ArgumentNullException(
                                                       nameof(webInterviewNotificationService));
            this.mediaStorage = mediaStorage;
            this.imageProcessingService = imageProcessingService;
        }

        public ActionResult AudioRecord(string interviewId, string fileName)
        {
            if (!Guid.TryParse(interviewId, out var id))
            {
                return HttpNotFound();
            }

            MultimediaFile file = null;
            if (fileName != null)
            {
                file = this.mediaStorage.Get(fileName, id);
            }

            if (file == null || file.Data.Length == 0)
                return HttpNotFound();

            return this.File(file.Data, file.MimeType, fileName);
        }

        [HttpPost]
        public async Task<ActionResult> Audio(string interviewId, string questionId, HttpPostedFileBase file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(interviewId);

            var questionIdentity = Identity.Parse(questionId);
            InterviewTreeQuestion question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question.IsAudio)
            {
                return this.Json("fail");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    byte[] bytes = ms.ToArray();

                    var fileName = $@"{question.VariableName}__{questionIdentity.RosterVector}.m4a";

                    var duration = SoundInfo.GetSoundLength(bytes);

                    mediaStorage.Store(new MultimediaFile
                    {
                        Filename = fileName,
                        Data = bytes,
                        Duration = duration,
                        MimeType = "audio/wav"
                    }, fileName, interview.Id);

                    var command = new AnswerAudioQuestionCommand(interview.Id,
                        interview.CurrentResponsibleId, questionIdentity.Id, questionIdentity.RosterVector,
                        DateTime.UtcNow, fileName, duration);

                    this.commandService.Execute(command);
                }
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId,
                    WebInterview.GetUiMessageFromException(e));
                throw;
            }

            return this.Json("ok");
        }

        [HttpPost]
        public async Task<ActionResult> Image(string interviewId, string questionId, HttpPostedFileBase file)
        {
            IStatefulInterview interview = this.statefulInterviewRepository.Get(interviewId);

            var questionIdentity = Identity.Parse(questionId);
            var question = interview.GetQuestion(questionIdentity);

            if (!interview.AcceptsInterviewerAnswers() && question.IsMultimedia)
            {
                return this.Json("fail");
            }

            string fileName = null;

            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    this.imageProcessingService.ValidateImage(ms.ToArray());

                    fileName = $@"{question.VariableName}{
                            string.Join(@"-", questionIdentity.RosterVector.Select(rv => rv))
                        }{DateTime.UtcNow.GetHashCode()}.jpg";
                    var responsibleId = interview.CurrentResponsibleId;

                    this.mediaStorage.Store(new MultimediaFile
                    {
                        Filename = fileName,
                        Data = ms.ToArray(),
                        MimeType = "image/jpg"
                    }, fileName, interview.Id);

                    this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                        responsibleId, questionIdentity.Id, questionIdentity.RosterVector, DateTime.UtcNow, fileName));
                }
            }
            catch (Exception e)
            {
                if (fileName != null)
                    webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId,
                        WebInterview.GetUiMessageFromException(e));
                throw;
            }

            return this.Json("ok");
        }
    }
}