using System;
using System.Web.Http;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Shared.Web.Filters;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [ApiNoCache]
    [RoutePrefix("api/webinterview/commands")]
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
        public override IHttpActionResult ChangeLanguage(Guid interviewId, [FromBody]ChangeLanguageRequest request) => base.ChangeLanguage(interviewId, request);

        [HttpPost]
        [Route("answerTextQuestion")]
        public override IHttpActionResult AnswerTextQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerTextListQuestion")]
        public override IHttpActionResult AnswerTextListQuestion(Guid interviewId, [FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerGpsQuestion")]
        public override IHttpActionResult AnswerGpsQuestion(Guid interviewId, [FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerDateQuestion")]
        public override IHttpActionResult AnswerDateQuestion(Guid interviewId, [FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerSingleOptionQuestion")]
        public override IHttpActionResult AnswerSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IHttpActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IHttpActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerMultiOptionQuestion")]
        public override IHttpActionResult AnswerMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerYesNoQuestion")]
        public override IHttpActionResult AnswerYesNoQuestion(Guid interviewId, [FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerIntegerQuestion")]
        public override IHttpActionResult AnswerIntegerQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerDoubleQuestion")]
        public override IHttpActionResult AnswerDoubleQuestion(Guid interviewId, [FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("answerQRBarcodeQuestion")]
        public override IHttpActionResult AnswerQRBarcodeQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(interviewId, answerRequest);

        [HttpPost]
        [Route("removeAnswer")]
        public override IHttpActionResult RemoveAnswer(Guid interviewId, [FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(interviewId, request);

        [HttpPost]
        [Route("sendNewComment")]
        public override IHttpActionResult SendNewComment(Guid interviewId, [FromBody]NewCommentRequest request) => base.SendNewComment(interviewId, request);

        [HttpPost]
        [Route("completeInterview")]
        public override IHttpActionResult CompleteInterview(Guid interviewId, [FromBody]CompleteInterviewRequest completeInterviewRequest)
        {
            evictionNotify.Evict(interviewId);
            return Ok();
        }
    }
}
