using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.Controllers
{
    public partial class WebInterviewController : BaseController
    {
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

                    var audioInfo = await this.audioProcessingService.CompressAudioFileAsync(bytes);

                    var fileName = $@"{question.VariableName}__{questionIdentity.RosterVector}.m4a";

                    audioFileStorage.StoreInterviewBinaryData(Guid.Parse(interviewId), fileName, audioInfo.Binary, audioInfo.MimeType);

                    var command = new AnswerAudioQuestionCommand(interview.Id,
                        interview.CurrentResponsibleId, questionIdentity.Id, questionIdentity.RosterVector,
                        fileName, audioInfo.Duration);

                    this.commandService.Execute(command);
                }
            }
            catch (Exception e)
            {
                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId, WebInterview.GetUiMessageFromException(e));
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

            string filename = null;

            try
            {
                using (var ms = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(ms);

                    this.imageProcessingService.ValidateImage(ms.ToArray());

                    filename = AnswerUtils.GetPictureFileName(question.VariableName, questionIdentity.RosterVector);
                    var responsibleId = interview.CurrentResponsibleId;

                    this.commandService.Execute(new AnswerPictureQuestionCommand(interview.Id,
                        responsibleId, questionIdentity.Id, questionIdentity.RosterVector, filename));

                    this.imageFileStorage.StoreInterviewBinaryData(interview.Id, filename, ms.ToArray(), file.ContentType);
                }
            }
            catch (Exception e) 
            {
                if(filename != null)
                    this.imageFileStorage.RemoveInterviewBinaryData(interview.Id, filename);

                webInterviewNotificationService.MarkAnswerAsNotSaved(interviewId, questionId, WebInterview.GetUiMessageFromException(e));
                throw;
            }
            return this.Json("ok");
        }
    }
}
