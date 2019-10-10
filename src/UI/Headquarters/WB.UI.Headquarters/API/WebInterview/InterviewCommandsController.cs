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
            this.authorizedUser.CanConductInterviewReview() 
            && this.Request.GetQueryNameValuePairs().SingleOrDefault(p => p.Key == @"review").Value.ToBool(false);


        protected override Guid GetCommandResponsibleId(Guid interviewId)
        {
            if (IsReviewMode())
                return this.authorizedUser.Id;

            var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
            return statefulInterview.CurrentResponsibleId;
        }

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult ChangeLanguage(ChangeLanguageRequest request) => base.ChangeLanguage(request);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerTextQuestion(Guid interviewId, string questionIdenty, string text) => base.AnswerTextQuestion(interviewId, questionIdenty, text);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerTextListQuestion(Guid interviewId, string questionIdenty, TextListAnswerRowDto[] rows) => base.AnswerTextListQuestion(interviewId, questionIdenty, rows);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerGpsQuestion(Guid interviewId, string questionIdenty, GpsAnswer answer) => base.AnswerGpsQuestion(interviewId, questionIdenty, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerDateQuestion(Guid interviewId, string questionIdenty, DateTime answer) => base.AnswerDateQuestion(interviewId, questionIdenty, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerSingleOptionQuestion(Guid interviewId, int answer, string questionId) => base.AnswerSingleOptionQuestion(interviewId, answer, questionId);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerLinkedSingleOptionQuestion(Guid interviewId, string questionIdentity, decimal[] answer) => base.AnswerLinkedSingleOptionQuestion(interviewId, questionIdentity, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerLinkedMultiOptionQuestion(Guid interviewId, string questionIdentity, decimal[][] answer) => base.AnswerLinkedMultiOptionQuestion(interviewId, questionIdentity, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerMultiOptionQuestion(Guid interviewId, int[] answer, string questionId) => base.AnswerMultiOptionQuestion(interviewId, answer, questionId);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerYesNoQuestion(Guid interviewId, string questionId, InterviewYesNoAnswer[] answerDto) => base.AnswerYesNoQuestion(interviewId, questionId, answerDto);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerIntegerQuestion(Guid interviewId, string questionIdenty, int answer) => base.AnswerIntegerQuestion(interviewId, questionIdenty, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerDoubleQuestion(Guid interviewId, string questionIdenty, double answer) => base.AnswerDoubleQuestion(interviewId, questionIdenty, answer);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult AnswerQRBarcodeQuestion(Guid interviewId, string questionIdenty, string text) => base.AnswerQRBarcodeQuestion(interviewId, questionIdenty, text);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult RemoveAnswer(Guid interviewId, string questionId) => base.RemoveAnswer(interviewId, questionId);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult SendNewComment(Guid interviewId, string questionIdentity, string comment) => base.SendNewComment(interviewId, questionIdentity, comment);

        [HttpPost]
        [ObserverNotAllowed]
        public override IHttpActionResult CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var interviewId = completeInterviewRequest.InterviewId;
            var command = new CompleteInterviewCommand(interviewId, GetCommandResponsibleId(interviewId), completeInterviewRequest.Comment);
            this.commandService.Execute(command);
            return Ok();
        }

        [HttpPost]
        [ObserverNotAllowed]
        public IHttpActionResult Approve(Guid interviewId, string comment)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), comment);

                this.commandService.Execute(command);
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), comment);
                this.commandService.Execute(command);
            }
            return Ok();
        }

        [HttpPost]
        [ObserverNotAllowed]
        public IHttpActionResult Reject(Guid interviewId, string comment, Guid? assignTo)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                if (assignTo.HasValue)
                {
                    var command = new RejectInterviewToInterviewerCommand(this.GetCommandResponsibleId(interviewId), interviewId, assignTo.Value, comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new RejectInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), comment);
                    this.commandService.Execute(command);
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var statefulInterview = statefulInterviewRepository.Get(interviewId.FormatGuid());
                if (statefulInterview.Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    var command = new UnapproveByHeadquartersCommand(interviewId, this.GetCommandResponsibleId(interviewId), comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new HqRejectInterviewCommand(interviewId, this.GetCommandResponsibleId(interviewId), comment);
                    this.commandService.Execute(command);
                }
            }
            return Ok();
        }

        [HttpPost]
        [ObserverNotAllowed]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        [Microsoft.AspNet.SignalR.Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public IHttpActionResult ResolveComment(Guid interviewId, string questionIdentity)
        {
            var identity = Identity.Parse(questionIdentity);
            var command = new ResolveCommentAnswerCommand(interviewId,
                this.GetCommandResponsibleId(interviewId),
                identity.Id,
                identity.RosterVector);

            this.commandService.Execute(command);
            return Ok();
        }
    }
}
