using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.SignalR.Hubs;
using SignalR.Extras.Autofac;
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
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    [WebInterviewAuthorize]
    public class WebInterviewHub : Enumerator.Native.WebInterview.WebInterview, ILifetimeHub
    {
        //private readonly IServiceLocator locator;
        
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewFactory interviewFactory;
        private readonly IStatefullInterviewSearcher statefullInterviewSearcher;
        private readonly IInterviewOverviewService overviewService;
        private IUnitOfWork unitOfWork;

        //separate interview logic into interface implementation from hub logic and inject it 
        //make sure that IServiceLocator is not used
        public WebInterviewHub(IStatefulInterviewRepository statefulInterviewRepository,
            ICommandService commandService,
            IQuestionnaireStorage questionnaireRepository,
            IWebInterviewNotificationService webInterviewNotificationService,
            IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IImageFileStorage imageFileStorage,
            IInterviewBrokenPackagesService interviewBrokenPackagesService,
            IAudioFileStorage audioFileStorage,
            IAuthorizedUser authorizedUser,
            IChangeStatusFactory changeStatusFactory,
            IInterviewFactory interviewFactory,
            IStatefullInterviewSearcher statefullInterviewSearcher,
            IInterviewOverviewService overviewService,
            IUnitOfWork unitOfWork) : base(statefulInterviewRepository,
            commandService,
            questionnaireRepository,
            webInterviewNotificationService,
            interviewEntityFactory,
            imageFileStorage,
            audioFileStorage)
        {
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
            this.authorizedUser = authorizedUser;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewFactory = interviewFactory;
            this.statefullInterviewSearcher = statefullInterviewSearcher;
            this.overviewService = overviewService;
            this.unitOfWork = unitOfWork;
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
            this.commandService.Execute(command);
        }

        protected override bool IsCurrentUserObserving => this.authorizedUser.IsObserving;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public SearchResults Search(FilterOption[] flags, int skip = 0, int limit = 50)
        {
            var interview = GetCallerInterview();
            var result = this.statefullInterviewSearcher.Search(interview, flags, skip, limit);
            return result;
        }

        public PagedApiResponse<OverviewNode> Overview(int skip, int take = 100)
        {
            take = Math.Min(take, 100);

            var interview = GetCallerInterview();
            var overview = this.overviewService.GetOverview(interview).ToList();
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
        
        public override InterviewInfo GetInterviewDetails()
        {
            var interviewDetails = base.GetInterviewDetails();
            interviewDetails.DoesBrokenPackageExist = this.interviewBrokenPackagesService.IsNeedShowBrokenPackageNotificationForInterview(Guid.Parse(this.CallerInterviewId));
            return interviewDetails;
        }
        
        public event EventHandler OnDisposing;

        protected override void Dispose(bool disposing)
        {
            //todo:af 
            //move it to more proper place
            
            unitOfWork.AcceptChanges();

            base.Dispose(disposing);
            if (disposing)
            {
                var handler = OnDisposing;
                handler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
