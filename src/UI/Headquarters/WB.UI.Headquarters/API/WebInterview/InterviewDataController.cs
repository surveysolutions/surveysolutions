using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNet.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class InterviewDataController : Enumerator.Native.WebInterview.Controllers.InterviewDataController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewOverviewService overviewService;
        private readonly IStatefullInterviewSearcher statefullInterviewSearcher;
        private readonly IInterviewFactory interviewFactory;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewBrokenPackagesService interviewBrokenPackagesService;

        public InterviewDataController(IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository,
            IWebInterviewNotificationService webInterviewNotificationService, IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IAuthorizedUser authorizedUser, IInterviewOverviewService overviewService, IStatefullInterviewSearcher statefullInterviewSearcher,
            IInterviewFactory interviewFactory, IChangeStatusFactory changeStatusFactory, IInterviewBrokenPackagesService interviewBrokenPackagesService) 
            : base(questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService, interviewEntityFactory)
        {
            this.authorizedUser = authorizedUser;
            this.overviewService = overviewService;
            this.statefullInterviewSearcher = statefullInterviewSearcher;
            this.interviewFactory = interviewFactory;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewBrokenPackagesService = interviewBrokenPackagesService;
        }

        protected override bool IsReviewMode() =>
            this.authorizedUser.CanConductInterviewReview() 
            && this.Request.GetQueryNameValuePairs().SingleOrDefault(p => p.Key == @"review").Value.ToBool(false);

        protected override bool IsCurrentUserObserving() => this.authorizedUser.IsObserving;

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public SearchResults Search(Guid interviewId, FilterOption[] flags, int skip = 0, int limit = 50)
        {
            var interview = GetCallerInterview(interviewId);
            var questionnaire = GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            var result = this.statefullInterviewSearcher.Search(interview, questionnaire, flags, skip, limit);
            return result;
        }

        public PagedApiResponse<OverviewNode> Overview(Guid interviewId, int skip, int take = 100)
        {
            take = Math.Min(take, 100);

            var interview = this.GetCallerInterview(interviewId);
            var questionnaire = this.GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            var overview = this.overviewService.GetOverview(interview, questionnaire, IsReviewMode()).ToList();
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

        public OverviewItemAdditionalInfo OverviewItemAdditionalInfo(Guid interviewId, string id)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            var currentUserId = this.authorizedUser.Id;
            var additionalInfo = this.overviewService.GetOverviewItemAdditionalInfo(statefulInterview, id, currentUserId);
            return additionalInfo;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public List<CommentedStatusHistoryView> GetStatusesHistory(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            return this.changeStatusFactory.GetFilteredStatuses(statefulInterview.Id);
        }

        [ObserverNotAllowed]
        public void SetFlag(Guid interviewId, string questionId, bool hasFlag)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            this.interviewFactory.SetFlagToQuestion(statefulInterview.Id, Identity.Parse(questionId), hasFlag);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @flags.js")]
        public IEnumerable<string> GetFlags(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            return this.interviewFactory.GetFlaggedQuestionIds(statefulInterview.Id).Select(x => x.ToString());
        }

        public override InterviewInfo GetInterviewDetails(Guid interviewId)
        {
            var interviewDetails = base.GetInterviewDetails(interviewId);
            interviewDetails.DoesBrokenPackageExist = this.interviewBrokenPackagesService.IsNeedShowBrokenPackageNotificationForInterview(interviewId);
            return interviewDetails;
        }
    }
}
