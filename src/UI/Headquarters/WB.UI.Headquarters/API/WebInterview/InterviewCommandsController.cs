using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.WebInterview
{
    [Authorize]
    [ApiNoCache]
    [CamelCase]
    [RoutePrefix("api/webinterview/commands")]
    public class InterviewCommandsController : CommandsController
    {
        private readonly IAuthorizedUser authorizedUser;

        public InterviewCommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, 
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IAuthorizedUser authorizedUser) 
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
        {
            this.authorizedUser = authorizedUser;
        }

        protected bool IsReviewMode() =>
            this.authorizedUser.CanConductInterviewReview() && this.Request.Headers.Contains(@"review");


        protected override Guid GetCommandResponsibleId(Guid interviewId)
        {
            if (IsReviewMode())
                return this.authorizedUser.Id;

            var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            return statefulInterview.CurrentResponsibleId;
        }

        [HttpPost]
        [ObserverNotAllowed]
        [Route("changeLanguage")]
        public override IHttpActionResult ChangeLanguage([FromBody]ChangeLanguageRequest request) => base.ChangeLanguage(request);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerTextQuestion")]
        public override IHttpActionResult AnswerTextQuestion([FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerTextListQuestion")]
        public override IHttpActionResult AnswerTextListQuestion([FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerGpsQuestion")]
        public override IHttpActionResult AnswerGpsQuestion([FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerDateQuestion")]
        public override IHttpActionResult AnswerDateQuestion([FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerSingleOptionQuestion")]
        public override IHttpActionResult AnswerSingleOptionQuestion([FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IHttpActionResult AnswerLinkedSingleOptionQuestion([FromBody] AnswerRequest< decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IHttpActionResult AnswerLinkedMultiOptionQuestion([FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerMultiOptionQuestion")]
        public override IHttpActionResult AnswerMultiOptionQuestion([FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerYesNoQuestion")]
        public override IHttpActionResult AnswerYesNoQuestion([FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerIntegerQuestion")]
        public override IHttpActionResult AnswerIntegerQuestion([FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerDoubleQuestion")]
        public override IHttpActionResult AnswerDoubleQuestion([FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("answerQRBarcodeQuestion")]
        public override IHttpActionResult AnswerQRBarcodeQuestion([FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(answerRequest);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("removeAnswer")]
        public override IHttpActionResult RemoveAnswer([FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(request);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("sendNewComment")]
        public override IHttpActionResult SendNewComment([FromBody]NewCommentRequest request) => base.SendNewComment(request);

        [HttpPost]
        [ObserverNotAllowed]
        [Route("completeInterview")]
        public override IHttpActionResult CompleteInterview([FromBody]CompleteInterviewRequest completeInterviewRequest)
        {
            var interviewId = completeInterviewRequest.InterviewId;
            var command = new CompleteInterviewCommand(interviewId, GetCommandResponsibleId(interviewId), completeInterviewRequest.Comment);
            this.commandService.Execute(command);
            return Ok();
        }


        public class ApproveInterviewRequest
        {
            public Guid InterviewId { get; set; }
            public string Comment { get; set; }
        }

        [HttpPost]
        [ObserverNotAllowed]
        [Route("approve")]
        public IHttpActionResult Approve([FromBody]ApproveInterviewRequest approveInterviewRequest)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(approveInterviewRequest.InterviewId, this.GetCommandResponsibleId(approveInterviewRequest.InterviewId), approveInterviewRequest.Comment);

                this.commandService.Execute(command);
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(approveInterviewRequest.InterviewId, this.GetCommandResponsibleId(approveInterviewRequest.InterviewId), approveInterviewRequest.Comment);
                this.commandService.Execute(command);
            }
            return Ok();
        }

        public class RejectInterviewRequest
        {
            public Guid InterviewId { get; set; }
            public string Comment { get; set; }
            public Guid? AssignTo { get; set; }
        }


        [HttpPost]
        [ObserverNotAllowed]
        [Route("reject")]
        public IHttpActionResult Reject([FromBody]RejectInterviewRequest rejectInterviewRequest)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                if (rejectInterviewRequest.AssignTo.HasValue)
                {
                    var command = new RejectInterviewToInterviewerCommand(this.GetCommandResponsibleId(rejectInterviewRequest.InterviewId), rejectInterviewRequest.InterviewId, rejectInterviewRequest.AssignTo.Value, rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new RejectInterviewCommand(rejectInterviewRequest.InterviewId, this.GetCommandResponsibleId(rejectInterviewRequest.InterviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var statefulInterview = statefulInterviewRepository.Get(rejectInterviewRequest.InterviewId.FormatGuid());
                if (statefulInterview.Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    var command = new UnapproveByHeadquartersCommand(rejectInterviewRequest.InterviewId, this.GetCommandResponsibleId(rejectInterviewRequest.InterviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new HqRejectInterviewCommand(rejectInterviewRequest.InterviewId, this.GetCommandResponsibleId(rejectInterviewRequest.InterviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
            }
            return Ok();
        }


        public class ResolveCommentRequest
        {
            public Guid InterviewId { get; set; }
            public string QuestionIdentity { get; set; }
        }

        [HttpPost]
        [ObserverNotAllowed]
        [Route("resolveComment")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        [Microsoft.AspNet.SignalR.Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IHttpActionResult ResolveComment([FromBody]ResolveCommentRequest resolveCommentRequest)
        {
            var identity = Identity.Parse(resolveCommentRequest.QuestionIdentity);
            var command = new ResolveCommentAnswerCommand(resolveCommentRequest.InterviewId,
                this.GetCommandResponsibleId(resolveCommentRequest.InterviewId),
                identity.Id,
                identity.RosterVector);

            this.commandService.Execute(command);
            return Ok();
        }
    }
}
