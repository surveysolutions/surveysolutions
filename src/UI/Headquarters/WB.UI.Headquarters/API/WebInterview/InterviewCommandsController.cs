using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.WebInterview
{
    [ApiNoCache]
    [WebInterviewDataAuthorize]
    [CamelCase]
    [RoutePrefix("api/webinterview/commands")]
    public class InterviewCommandsController : CommandsController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewFactory interviewFactory;

        public InterviewCommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, 
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IAuthorizedUser authorizedUser, IInterviewFactory interviewFactory) 
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
        {
            this.authorizedUser = authorizedUser;
            this.interviewFactory = interviewFactory;
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
        [WebInterviewObserverNotAllowed]
        [Route("changeLanguage")]
        public override IHttpActionResult ChangeLanguage(Guid interviewId, [FromBody]ChangeLanguageRequest request) => base.ChangeLanguage(interviewId, request);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerTextQuestion")]
        public override IHttpActionResult AnswerTextQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerTextListQuestion")]
        public override IHttpActionResult AnswerTextListQuestion(Guid interviewId, [FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerGpsQuestion")]
        public override IHttpActionResult AnswerGpsQuestion(Guid interviewId, [FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerDateQuestion")]
        public override IHttpActionResult AnswerDateQuestion(Guid interviewId, [FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerSingleOptionQuestion")]
        public override IHttpActionResult AnswerSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IHttpActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest< decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IHttpActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerMultiOptionQuestion")]
        public override IHttpActionResult AnswerMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerYesNoQuestion")]
        public override IHttpActionResult AnswerYesNoQuestion(Guid interviewId, [FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerIntegerQuestion")]
        public override IHttpActionResult AnswerIntegerQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerDoubleQuestion")]
        public override IHttpActionResult AnswerDoubleQuestion(Guid interviewId, [FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("answerQRBarcodeQuestion")]
        public override IHttpActionResult AnswerQRBarcodeQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(interviewId, answerRequest);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("removeAnswer")]
        public override IHttpActionResult RemoveAnswer(Guid interviewId, [FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(interviewId, request);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("sendNewComment")]
        public override IHttpActionResult SendNewComment(Guid interviewId, [FromBody]NewCommentRequest request) => base.SendNewComment(interviewId, request);

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("completeInterview")]
        public override IHttpActionResult CompleteInterview(Guid interviewId, [FromBody]CompleteInterviewRequest completeInterviewRequest)
        {
            var command = new CompleteInterviewCommand(interviewId, GetCommandResponsibleId(interviewId), completeInterviewRequest.Comment);
            this.commandService.Execute(command);
            return Ok();
        }


        public class ApproveInterviewRequest
        {
            public string Comment { get; set; }
        }

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("approve")]
        public IHttpActionResult Approve(Guid interviewId, [FromBody]ApproveInterviewRequest approveInterviewRequest)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), approveInterviewRequest.Comment);

                this.commandService.Execute(command);
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), approveInterviewRequest.Comment);
                this.commandService.Execute(command);
            }
            return Ok();
        }

        public class RejectInterviewRequest
        {
            public string Comment { get; set; }
            public Guid? AssignTo { get; set; }
        }


        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("reject")]
        public IHttpActionResult Reject(Guid interviewId, [FromBody]RejectInterviewRequest rejectInterviewRequest)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                if (rejectInterviewRequest.AssignTo.HasValue)
                {
                    var command = new RejectInterviewToInterviewerCommand(this.GetCommandResponsibleId(interviewId), interviewId, rejectInterviewRequest.AssignTo.Value, rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new RejectInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
                if (statefulInterview.Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    var command = new UnapproveByHeadquartersCommand(interviewId, this.GetCommandResponsibleId(interviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new HqRejectInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), rejectInterviewRequest.Comment);
                    this.commandService.Execute(command);
                }
            }
            return Ok();
        }


        public class ResolveCommentRequest
        {
            public string QuestionId { get; set; }
        }

        [HttpPost]
        [WebInterviewObserverNotAllowed]
        [Route("resolveComment")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IHttpActionResult ResolveComment(Guid interviewId, [FromBody]ResolveCommentRequest resolveCommentRequest)
        {
            var identity = Identity.Parse(resolveCommentRequest.QuestionId);
            var command = new ResolveCommentAnswerCommand(interviewId,
                this.GetCommandResponsibleId(interviewId),
                identity.Id,
                identity.RosterVector);

            this.commandService.Execute(command);
            return Ok();
        }

        public class SetFlagRequest
        {
            public string QuestionId { get; set; }
            public bool HasFlag { get; set; }
        }


        [HttpPost]
        [Route("setFlag")]
        [WebInterviewObserverNotAllowed]
        public IHttpActionResult SetFlag(Guid interviewId, [FromBody] SetFlagRequest request)
        {
            this.interviewFactory.SetFlagToQuestion(interviewId, Identity.Parse(request.QuestionId), request.HasFlag);
            return Ok();
        }
    }
}
