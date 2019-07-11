﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    [WebInterviewAuthorize]
    public class WebInterviewHub : Enumerator.Native.WebInterview.WebInterview, ILifetimeHub
    {
        private IInterviewBrokenPackagesService interviewBrokenPackagesService => this.ServiceLocator.GetInstance<IInterviewBrokenPackagesService>();
        private IAuthorizedUser authorizedUser => this.ServiceLocator.GetInstance<IAuthorizedUser>();
        private IChangeStatusFactory changeStatusFactory => this.ServiceLocator.GetInstance<IChangeStatusFactory>();
        private IInterviewFactory interviewFactory => this.ServiceLocator.GetInstance<IInterviewFactory>();
        private IStatefullInterviewSearcher statefullInterviewSearcher => this.ServiceLocator.GetInstance<IStatefullInterviewSearcher>();
        private IInterviewOverviewService overviewService => this.ServiceLocator.GetInstance<IInterviewOverviewService>();
        
        protected override bool IsReviewMode =>
            this.authorizedUser.CanConductInterviewReview() &&
            this.Context.QueryString[@"review"].ToBool(false);

        protected override Guid CommandResponsibleId
        {
            get
            {
                if (IsReviewMode)
                {
                    return this.authorizedUser.Id;
                }

                var statefulInterview = this.GetCallerInterview();
                return statefulInterview.CurrentResponsibleId;
            }
        }

        [ObserverNotAllowed]
        public override void CompleteInterview(CompleteInterviewRequest completeInterviewRequest)
        {
            var command = new CompleteInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, completeInterviewRequest.Comment);
            this.commandService.Execute(command);
        }

        protected override bool IsCurrentUserObserving => this.authorizedUser.IsObserving;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public SearchResults Search(FilterOption[] flags, int skip = 0, int limit = 50)
        {
            var interview = GetCallerInterview();
            var questionnaire = GetCallerQuestionnaire();
            var result = this.statefullInterviewSearcher.Search(interview, questionnaire, flags, skip, limit);
            return result;
        }

        public PagedApiResponse<OverviewNode> Overview(int skip, int take = 100)
        {
            take = Math.Min(take, 100);

            var interview = this.GetCallerInterview();
            var questionnaire = this.GetCallerQuestionnaire();
            var overview = this.overviewService.GetOverview(interview, questionnaire, IsReviewMode).ToList();
            var result = overview.Skip(skip).Take(take).ToList();

            var isLastPage = skip + result.Count >= overview.Count;

            return new PagedApiResponse<OverviewNode>
            {
                Items = result,
                Count = result.Count,
                IsLastPage = isLastPage,
                Skip = skip,
                Total = overview.Count
            };
        }

        public OverviewItemAdditionalInfo OverviewItemAdditionalInfo(string id)
        {
            var statefulInterview = this.GetCallerInterview();
            var currentUserId = this.authorizedUser.Id;
            var additionalInfo = this.overviewService.GetOverviewItemAdditionalInfo(statefulInterview, id, currentUserId);
            return additionalInfo;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public List<CommentedStatusHistoryView> GetStatusesHistory()
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

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @flags.js")]
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
                var command = new ApproveInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);

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
                    var command = new RejectInterviewToInterviewerCommand(this.CommandResponsibleId, this.GetCallerInterview().Id, assignTo.Value, comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new RejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    this.commandService.Execute(command);
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                if (this.GetCallerInterview().Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    var command = new UnapproveByHeadquartersCommand(GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    this.commandService.Execute(command);
                }
                else
                {
                    var command = new HqRejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    this.commandService.Execute(command);
                }
            }
        }
        
        [ObserverNotAllowed]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @store.actions.js")]
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public void ResolveComment(string questionIdentity, Guid commentId)
        {
            var identity = Identity.Parse(questionIdentity);
            var command = new ResolveCommentAnswerCommand(this.GetCallerInterview().Id, 
                this.CommandResponsibleId, 
                identity.Id, 
                identity.RosterVector,
                commentId);

            this.commandService.Execute(command);
        }

        public override InterviewInfo GetInterviewDetails()
        {
            var interviewDetails = base.GetInterviewDetails();
            interviewDetails.DoesBrokenPackageExist = this.interviewBrokenPackagesService.IsNeedShowBrokenPackageNotificationForInterview(Guid.Parse(this.CallerInterviewId));
            return interviewDetails;
        }
        
        public event EventHandler OnDisposing;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                var handler = OnDisposing;
                handler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
