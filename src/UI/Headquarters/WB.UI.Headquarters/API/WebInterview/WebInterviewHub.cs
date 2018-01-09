﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API.WebInterview.Pipeline;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    [WebInterviewAuthorize]
    public class WebInterviewHub : Enumerator.Native.WebInterview.WebInterview
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewFactory interviewFactory;
        private readonly IStatefullInterviewSearcher statefullInterviewSearcher;

        public WebInterviewHub(IStatefulInterviewRepository statefulInterviewRepository, ICommandService commandService, IQuestionnaireStorage questionnaireRepository, IWebInterviewNotificationService webInterviewNotificationService, IAuthorizedUser authorizedUser, IChangeStatusFactory changeStatusFactory, IInterviewFactory interviewFactory, IStatefullInterviewSearcher statefullInterviewSearcher, IWebInterviewInterviewEntityFactory interviewEntityFactory) : 
            base(statefulInterviewRepository, commandService, questionnaireRepository, webInterviewNotificationService, interviewEntityFactory)
        {
            this.authorizedUser = authorizedUser;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewFactory = interviewFactory;
            this.statefullInterviewSearcher = statefullInterviewSearcher;
        }

        protected override bool IsReviewMode =>
            this.authorizedUser.CanConductInterviewReview() &&
            this.Context.QueryString[@"review"].ToBool(false);

        protected override Guid CommandResponsibleId
        {
            get
            {
                var statefulInterview = this.GetCallerInterview();
                if (IsReviewMode)
                {
                    return this.authorizedUser.Id;
                }

                return statefulInterview.CurrentResponsibleId;
            }
        }

        [ObserverNotAllowed]
        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var command = new CompleteInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, completeInterviewRequest.Comment, DateTime.UtcNow);
            this.commandService.Execute(command);
        }

        protected override bool IsCurrentUserObserving => this.authorizedUser.IsObserving;

        public SearchResults Search(FilterOption[] flags, int skip = 0, int limit = 50)
        {
            var interview = GetCallerInterview();
            var result = this.statefullInterviewSearcher.Search(interview, flags, skip, limit);
            return result;
        }

        public List<CommentedStatusHistroyView> GetStatusesHistory()
        {
            var statefulInterview = this.GetCallerInterview();
            return this.changeStatusFactory.GetFilteredStatuses(statefulInterview.Id);
        }

        [ObserverNotAllowed]
        public void SetFlag(string questionId, bool hasFlag)
        {
            var statefulInterview = this.GetCallerInterview();
            this.interviewFactory.SetFlagToQuestion(statefulInterview.Id, Identity.Parse(questionId), hasFlag);
        }

        public IEnumerable<string> GetFlags()
        {
            var statefulInterview = this.GetCallerInterview();
            return this.interviewFactory.GetFlaggedQuestionIds(statefulInterview.Id).Select(x => x.ToString());
        }

        [ObserverNotAllowed]
        public void Approve(string comment)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment, DateTime.UtcNow);
                this.commandService.Execute(command);
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                this.commandService.Execute(command);
            }
        }

        [ObserverNotAllowed]
        public void Reject(string comment, Guid? assignTo)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                if (assignTo.HasValue)
                {
                    var command = new RejectInterviewToInterviewerCommand(this.CommandResponsibleId, this.GetCallerInterview().Id, assignTo.Value, comment, DateTime.UtcNow);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new RejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment, DateTime.UtcNow);
                    this.commandService.Execute(command);
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqRejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                this.commandService.Execute(command);
            }

        }
    }
}