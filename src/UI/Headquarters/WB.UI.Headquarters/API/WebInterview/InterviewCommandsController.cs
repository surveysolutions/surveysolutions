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

namespace WB.UI.Headquarters.API.WebInterview
{
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

        [ObserverNotAllowed]
        public override IHttpActionResult CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var interviewId = completeInterviewRequest.InterviewId;
            var command = new CompleteInterviewCommand(interviewId, GetCommandResponsibleId(interviewId), completeInterviewRequest.Comment);
            this.commandService.Execute(command);
            return Ok();
        }

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
