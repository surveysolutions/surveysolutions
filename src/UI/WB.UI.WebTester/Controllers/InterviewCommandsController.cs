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
        public override IHttpActionResult ChangeLanguage([FromBody] ChangeLanguageRequest request) => base.ChangeLanguage(request);

        [HttpPost]
        [Route("answerTextQuestion")]
        public override IHttpActionResult AnswerTextQuestion([FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(answerRequest);

        [HttpPost]
        [Route("answerTextListQuestion")]
        public override IHttpActionResult AnswerTextListQuestion([FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(answerRequest);

        [HttpPost]
        [Route("answerGpsQuestion")]
        public override IHttpActionResult AnswerGpsQuestion([FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(answerRequest);

        [HttpPost]
        [Route("answerDateQuestion")]
        public override IHttpActionResult AnswerDateQuestion([FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(answerRequest);

        [HttpPost]
        [Route("answerSingleOptionQuestion")]
        public override IHttpActionResult AnswerSingleOptionQuestion([FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(answerRequest);

        [HttpPost]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IHttpActionResult AnswerLinkedSingleOptionQuestion([FromBody] AnswerRequest<decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(answerRequest);

        [HttpPost]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IHttpActionResult AnswerLinkedMultiOptionQuestion([FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(answerRequest);

        [HttpPost]
        [Route("answerMultiOptionQuestion")]
        public override IHttpActionResult AnswerMultiOptionQuestion([FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(answerRequest);

        [HttpPost]
        [Route("answerYesNoQuestion")]
        public override IHttpActionResult AnswerYesNoQuestion([FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(answerRequest);

        [HttpPost]
        [Route("answerIntegerQuestion")]
        public override IHttpActionResult AnswerIntegerQuestion([FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(answerRequest);

        [HttpPost]
        [Route("answerDoubleQuestion")]
        public override IHttpActionResult AnswerDoubleQuestion([FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(answerRequest);

        [HttpPost]
        [Route("answerQRBarcodeQuestion")]
        public override IHttpActionResult AnswerQRBarcodeQuestion([FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(answerRequest);

        [HttpPost]
        [Route("removeAnswer")]
        public override IHttpActionResult RemoveAnswer([FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(request);

        [HttpPost]
        [Route("sendNewComment")]
        public override IHttpActionResult SendNewComment([FromBody]NewCommentRequest request) => base.SendNewComment(request);

        [HttpPost]
        [Route("completeInterview")]
        public override IHttpActionResult CompleteInterview([FromBody]CompleteInterviewRequest completeInterviewRequest)
        {
            evictionNotify.Evict(completeInterviewRequest.InterviewId);
            return Ok();
        }
    }
}
