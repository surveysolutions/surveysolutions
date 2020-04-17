﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [ApiNoCache]
    [WebInterviewAuthorize(InterviewIdQueryString = "interviewId")]
    [Route("api/webinterview/commands")]
    [ObserverNotAllowed]
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
            this.authorizedUser.CanConductInterviewReview() && this.Request.Headers.ContainsKey("review");


        protected override Guid GetCommandResponsibleId(Guid interviewId)
        {
            if (IsReviewMode())
                return this.authorizedUser.Id;

            var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            return statefulInterview.CurrentResponsibleId;
        }

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("changeLanguage")]
        public override IActionResult ChangeLanguage(Guid interviewId, [FromBody]ChangeLanguageRequest request) => base.ChangeLanguage(interviewId, request);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerTextQuestion")]
        public override IActionResult AnswerTextQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerTextQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerTextListQuestion")]
        public override IActionResult AnswerTextListQuestion(Guid interviewId, [FromBody] AnswerRequest<TextListAnswerRowDto[]> answerRequest) => base.AnswerTextListQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerGpsQuestion")]
        public override IActionResult AnswerGpsQuestion(Guid interviewId, [FromBody] AnswerRequest<GpsAnswer> answerRequest) => base.AnswerGpsQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerDateQuestion")]
        public override IActionResult AnswerDateQuestion(Guid interviewId, [FromBody] AnswerRequest<DateTime> answerRequest) => base.AnswerDateQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerSingleOptionQuestion")]
        public override IActionResult AnswerSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerLinkedSingleOptionQuestion")]
        public override IActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, [FromBody] AnswerRequest< decimal[]> answerRequest) => base.AnswerLinkedSingleOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerLinkedMultiOptionQuestion")]
        public override IActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<decimal[][]> answerRequest) => base.AnswerLinkedMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerMultiOptionQuestion")]
        public override IActionResult AnswerMultiOptionQuestion(Guid interviewId, [FromBody] AnswerRequest<int[]> answerRequest) => base.AnswerMultiOptionQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerYesNoQuestion")]
        public override IActionResult AnswerYesNoQuestion(Guid interviewId, [FromBody] AnswerRequest<InterviewYesNoAnswer[]> answerRequest) => base.AnswerYesNoQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerIntegerQuestion")]
        public override IActionResult AnswerIntegerQuestion(Guid interviewId, [FromBody] AnswerRequest<int> answerRequest) => base.AnswerIntegerQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerDoubleQuestion")]
        public override IActionResult AnswerDoubleQuestion(Guid interviewId, [FromBody] AnswerRequest<double> answerRequest) => base.AnswerDoubleQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("answerQRBarcodeQuestion")]
        public override IActionResult AnswerQRBarcodeQuestion(Guid interviewId, [FromBody] AnswerRequest<string> answerRequest) => base.AnswerQRBarcodeQuestion(interviewId, answerRequest);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("removeAnswer")]
        public override IActionResult RemoveAnswer(Guid interviewId, [FromBody]RemoveAnswerRequest request) => base.RemoveAnswer(interviewId, request);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("sendNewComment")]
        public override IActionResult SendNewComment(Guid interviewId, [FromBody]NewCommentRequest request) => base.SendNewComment(interviewId, request);

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("completeInterview")]
        public override IActionResult CompleteInterview(Guid interviewId, [FromBody]CompleteInterviewRequest completeInterviewRequest)
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
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("approve")]
        public IActionResult Approve(Guid interviewId, [FromBody]ApproveInterviewRequest approveInterviewRequest)
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
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        [Route("reject")]
        public IActionResult Reject(Guid interviewId, [FromBody]RejectInterviewRequest rejectInterviewRequest)
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


        public class ResolveCommentRequest : AnswerRequest
        {
        }

        [HttpPost]
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
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
        [Enumerator.Native.WebInterview.ObserverNotAllowed]
        public IActionResult SetFlag(Guid interviewId, [FromBody] SetFlagRequest request)
        {
            this.interviewFactory.SetFlagToQuestion(interviewId, Identity.Parse(request.Identity), request.HasFlag);
            return Ok();
        }
    }
}
