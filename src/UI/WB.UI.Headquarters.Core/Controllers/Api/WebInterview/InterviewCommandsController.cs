using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.CalendarEvents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [ApiNoCache]
    [WebInterviewAuthorize(InterviewIdQueryString = "interviewId")]
    [Route("api/webinterview/commands")]
    [ObservingNotAllowed]
    public class InterviewCommandsController : CommandsController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewFactory interviewFactory;
        private readonly IUserViewFactory userViewFactory;
        private readonly ICalendarEventService calendarEventService;

        public InterviewCommandsController(ICommandService commandService, IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, 
            IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IAuthorizedUser authorizedUser, IInterviewFactory interviewFactory,
            IUserViewFactory userViewFactory, ICalendarEventService calendarEventService) 
            : base(commandService, imageFileStorage, audioFileStorage, questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService)
        {
            this.authorizedUser = authorizedUser;
            this.interviewFactory = interviewFactory;
            this.userViewFactory = userViewFactory;
            this.calendarEventService = calendarEventService;
        }

        protected bool IsReviewMode() =>
            this.authorizedUser.CanConductInterviewReview() && this.Request.Headers.ContainsKey("review");


        protected override Guid GetCommandResponsibleId(Guid interviewId)
        {
            if (IsReviewMode())
                return this.authorizedUser.Id;

            var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            return statefulInterview.CurrentResponsibleId;
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
        public override IActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest< decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(interviewId, answerRequest);

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
            ICommand command = new CompleteInterviewCommand(interviewId, GetCommandResponsibleId(interviewId), completeInterviewRequest.Comment);
            this.commandService.Execute(command);
            return Ok();
        }

        public class RequestWebInterviewRequest : AnswerRequest
        {
            public string Comment { get; set; }
        }

        [HttpPost]
        [Route("requestWebInterview")]
        public IActionResult RequestWebInterview(Guid interviewId, [FromBody]RequestWebInterviewRequest completeInterviewRequest)
        {
            ICommand command = new RequestWebInterviewCommand(interviewId, GetCommandResponsibleId(interviewId),
                completeInterviewRequest.Comment);
                
            this.commandService.Execute(command);
            return Ok();
        }

        public class ApproveInterviewRequest
        {
            public string Comment { get; set; }
        }

        [HttpPost]
        [Route("approve")]
        public IActionResult Approve(Guid interviewId, [FromBody]ApproveInterviewRequest approveInterviewRequest)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), approveInterviewRequest.Comment);

                this.commandService.Execute(command);

                CompleteCalendarEventIfExists(interviewId);
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), approveInterviewRequest.Comment);
                this.commandService.Execute(command);
                CompleteCalendarEventIfExists(interviewId);
            }
            return Ok();
        }

        private void CompleteCalendarEventIfExists(Guid interviewId)
        {
            var calendarEvent = calendarEventService.GetActiveCalendarEventForInterviewId(interviewId);
            if (calendarEvent != null && !calendarEvent.IsCompleted())
                this.commandService.Execute(new CompleteCalendarEventCommand(calendarEvent.PublicKey, this.GetCommandResponsibleId(interviewId)));
        }

        public class RejectInterviewRequest
        {
            public string Comment { get; set; }
            public Guid? AssignTo { get; set; }
        }

        [HttpPost]
        [Route("reject")]
        public IActionResult Reject(Guid interviewId, [FromBody]RejectInterviewRequest rejectInterviewRequest)
        {
            var commandResponsibleId = this.GetCommandResponsibleId(interviewId);
            ICommand command = null;

            if (this.authorizedUser.IsSupervisor)
            {
                if (rejectInterviewRequest.AssignTo.HasValue)
                {
                    command = new RejectInterviewToInterviewerCommand(commandResponsibleId, interviewId, rejectInterviewRequest.AssignTo.Value, rejectInterviewRequest.Comment);
                }
                else
                {
                    command = new RejectInterviewCommand(interviewId, commandResponsibleId, rejectInterviewRequest.Comment);
                }
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
                if (statefulInterview.Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    command = new UnapproveByHeadquartersCommand(interviewId, commandResponsibleId, rejectInterviewRequest.Comment);
                }
                else
                {
                    if (rejectInterviewRequest.AssignTo.HasValue)
                    {
                        var newResponsible = userViewFactory.GetUser(rejectInterviewRequest.AssignTo.Value);
                        if (newResponsible.IsInterviewer())
                            command = new HqRejectInterviewToInterviewerCommand(interviewId,commandResponsibleId, rejectInterviewRequest.AssignTo.Value, newResponsible.Supervisor.Id, rejectInterviewRequest.Comment);
                        else if (newResponsible.IsSupervisor())
                            command = new HqRejectInterviewToSupervisorCommand(interviewId, commandResponsibleId, rejectInterviewRequest.AssignTo.Value, rejectInterviewRequest.Comment);
                    }
                    else
                    {
                        command = new HqRejectInterviewCommand(interviewId, commandResponsibleId, rejectInterviewRequest.Comment);
                    }
                }
            }

            if (command != null)
            {
                this.commandService.Execute(command);
                CompleteCalendarEventIfExists(interviewId);
            }
            return Ok();
        }


        public class ResolveCommentRequest : AnswerRequest
        {
        }

        [HttpPost]
        [Route("resolveComment")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IActionResult ResolveComment(Guid interviewId, [FromBody]ResolveCommentRequest resolveCommentRequest)
        {
            var identity = Identity.Parse(resolveCommentRequest.Identity);
            var command = new ResolveCommentAnswerCommand(interviewId,
                this.GetCommandResponsibleId(interviewId),
                identity.Id,
                identity.RosterVector);

            this.commandService.Execute(command);
            return Ok();
        }

        public class SetFlagRequest : AnswerRequest
        {
            public bool HasFlag { get; set; }
        }

        [HttpPost]
        [Route("setFlag")]
        public IActionResult SetFlag(Guid interviewId, [FromBody] SetFlagRequest request)
        {
            this.interviewFactory.SetFlagToQuestion(interviewId, Identity.Parse(request.Identity), request.HasFlag);
            return Ok();
        }
    }
}
