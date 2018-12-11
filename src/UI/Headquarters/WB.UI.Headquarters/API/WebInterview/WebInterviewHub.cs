using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        //private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;
        private readonly IAuthorizedUser authorizedUser;
        //private readonly IChangeStatusFactory changeStatusFactory;
        //private readonly IInterviewFactory interviewFactory;
        //private readonly IStatefullInterviewSearcher statefullInterviewSearcher;
        private readonly IInterviewOverviewService overviewService;
        
        public WebInterviewHub(
            //IStatefulInterviewRepository statefulInterviewRepository,
            //ICommandService commandService,
            //IQuestionnaireStorage questionnaireRepository,
            //IWebInterviewNotificationService webInterviewNotificationService,
            //IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IImageFileStorage imageFileStorage,
            //IInterviewBrokenPackagesService interviewBrokenPackagesService,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            //IChangeStatusFactory changeStatusFactory,
            //IInterviewFactory interviewFactory,
            //IStatefullInterviewSearcher statefullInterviewSearcher,
            IInterviewOverviewService overviewService) : base(
            //statefulInterviewRepository,
            //commandService,
            //questionnaireRepository,
            //webInterviewNotificationService,
            //interviewEntityFactory,
            imageFileStorage,
            audioFileStorage)
        {
            //this.interviewBrokenPackagesService = interviewBrokenPackagesService;
            this.authorizedUser = authorizedUser;
            //this.changeStatusFactory = changeStatusFactory;
            //this.interviewFactory = interviewFactory;
            //this.statefullInterviewSearcher = statefullInterviewSearcher;
            this.overviewService = overviewService;
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
            var command = new CompleteInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, completeInterviewRequest.Comment);
            InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
        }

        protected override bool IsCurrentUserObserving => this.authorizedUser.IsObserving;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public SearchResults Search(FilterOption[] flags, int skip = 0, int limit = 50)
        {
            SearchResults result = null;
            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                var statefulInterview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
                result = sl.GetInstance<IStatefullInterviewSearcher>().Search(statefulInterview, flags, skip, limit);
            });

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
            List<CommentedStatusHistoryView> flags = new List<CommentedStatusHistoryView>();

            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                var statefulInterview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
                flags = sl.GetInstance<IChangeStatusFactory>().GetFilteredStatuses(statefulInterview.Id);
            });

            return flags;
        }

        [ObserverNotAllowed]
        public void SetFlag(string questionId, bool hasFlag)
        {
            InScopeExecutor.Current.ExecuteActionInScope(sl =>
                {
                    var statefulInterview = GetCallerInterview(sl.GetInstance<IStatefulInterviewRepository>());
                    sl.GetInstance<IInterviewFactory>().SetFlagToQuestion(statefulInterview.Id, Identity.Parse(questionId), hasFlag);
                }
            );
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @flags.js")]
        public IEnumerable<string> GetFlags()
        {
            var statefulInterview = this.GetCallerInterview();
            List<string> flags = new List<string>();

            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                flags = sl.GetInstance<IInterviewFactory>().GetFlaggedQuestionIds(statefulInterview.Id)
                    .Select(x => x.ToString()).ToList();
            });

            return flags;
        }

        [ObserverNotAllowed]
        public void Approve(string comment)
        {
            if (this.authorizedUser.IsSupervisor)
            {
                var command = new ApproveInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);

                InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
            }
            else if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                var command = new HqApproveInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
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
                    InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
                }
                else
                {
                    var command = new RejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
                }
            }
            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
            {
                if (this.GetCallerInterview().Status == InterviewStatus.ApprovedByHeadquarters)
                {
                    var command = new UnapproveByHeadquartersCommand(GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
                }
                else
                {
                    var command = new HqRejectInterviewCommand(this.GetCallerInterview().Id, this.CommandResponsibleId, comment);
                    InScopeExecutor.Current.ExecuteActionInScope(sl => sl.GetInstance<ICommandService>().Execute(command));
                }
            }
        }
        
        public override InterviewInfo GetInterviewDetails()
        {
            InterviewInfo interviewDetails = base.GetInterviewDetails(); ;

            InScopeExecutor.Current.ExecuteActionInScope(sl =>
            {
                interviewDetails.DoesBrokenPackageExist = sl.GetInstance<IInterviewBrokenPackagesService>().IsNeedShowBrokenPackageNotificationForInterview(Guid.Parse(this.CallerInterviewId));
            });
            
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
