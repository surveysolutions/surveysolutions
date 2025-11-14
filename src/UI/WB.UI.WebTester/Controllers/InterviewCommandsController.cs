using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [ResponseCache(NoStore = true)]
    [Route("api/webinterview/commands")]
    public class InterviewCommandsController : CommandsController
    {
        private readonly IEvictionNotifier evictionNotify;

        public InterviewCommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, 
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IEvictionNotifier evictionNotify) 
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
        {
            this.evictionNotify = evictionNotify;
        }

        [HttpPost]
        [Route("changeLanguage")]
        public override IActionResult ChangeLanguage(Guid interviewId, [FromBody]ChangeLanguageRequest request) => base.ChangeLanguage(interviewId, request);

        [HttpPost]
        [Route("answerTextQuestion")]
        public override IActionResult AnswerTextQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerTextListQuestion")]
        public override IActionResult AnswerTextListQuestion(Guid interviewId, [FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerGpsQuestion")]
        public override IActionResult AnswerGpsQuestion(Guid interviewId, [FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerDateQuestion")]
        public override IActionResult AnswerDateQuestion(Guid interviewId, [FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerSingleOptionQuestion")]
        public override IActionResult AnswerSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerMultiOptionQuestion")]
        public override IActionResult AnswerMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerYesNoQuestion")]
        public override IActionResult AnswerYesNoQuestion(Guid interviewId, [FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerIntegerQuestion")]
        public override IActionResult AnswerIntegerQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerDoubleQuestion")]
        public override IActionResult AnswerDoubleQuestion(Guid interviewId, [FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerQRBarcodeQuestion")]
        public override IActionResult AnswerQRBarcodeQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("removeAnswer")]
        public override IActionResult RemoveAnswer(Guid interviewId, [FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(interviewId, request);

        [HttpPost]
        [Route("sendNewComment")]
        public override IActionResult SendNewComment(Guid interviewId, [FromBody]NewCommentRequest request) => base.SendNewComment(interviewId, request);

        [HttpPost]
        [Route("completeInterview")]
        public override IActionResult CompleteInterview(Guid interviewId, [FromBody]CompleteInterviewRequest completeInterviewRequest)
        {
            evictionNotify.Evict(interviewId);
            return Ok();
        }

        [HttpPost]
        [Route("prepareCompleteInterview")]
        public override IActionResult PrepareCompleteInterview(Guid interviewId)
        {
            return Ok();
        }
    }
}
