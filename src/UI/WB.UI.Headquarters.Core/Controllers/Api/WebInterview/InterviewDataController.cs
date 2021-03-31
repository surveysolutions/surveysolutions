using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;
using InterviewEntity = WB.Enumerator.Native.WebInterview.Models.InterviewEntity;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [ApiNoCache]
    [WebInterviewAuthorize(InterviewIdQueryString = "interviewId")]
    [Route("api/webinterview")]
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
            this.authorizedUser.CanConductInterviewReview() && this.Request.Headers.ContainsKey(@"review");

        protected override bool IsCurrentUserObserving() => this.authorizedUser.IsObserving;

        [HttpGet]
        [Route("getLanguageInfo")]
        public override LanguageInfo GetLanguageInfo(Guid interviewId) => base.GetLanguageInfo(interviewId);

        [HttpGet]
        [Route("getTopFilteredOptionsForQuestion")]
        public override DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count)
            => base.GetTopFilteredOptionsForQuestion(interviewId, id, filter, count);

        [HttpGet]
        [Route("getTopFilteredOptionsForQuestionWithExclude")]
        public override DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count, [FromQuery(Name = "excludedOptionIds[]")] int[] excludedOptionIds = null) 
            => base.GetTopFilteredOptionsForQuestion(interviewId, id, filter, count, excludedOptionIds);

        [HttpGet]
        [Route("getBreadcrumbs")]
        public override BreadcrumbInfo GetBreadcrumbs(Guid interviewId, string sectionId = null) => base.GetBreadcrumbs(interviewId, sectionId);

        [HttpGet]
        [Route("getCompleteInfo")]
        public override CompleteInfo GetCompleteInfo(Guid interviewId) => base.GetCompleteInfo(interviewId);

        [HttpGet]
        [Route("getCoverInfo")]
        public override CoverInfo GetCoverInfo(Guid interviewId) => base.GetCoverInfo(interviewId);

        [HttpGet]
        [Route("getEntitiesDetails")]
        public override InterviewEntity[] GetEntitiesDetails(Guid interviewId, [FromQuery(Name = "ids[]")] string[] ids, string sectionId = null) => base.GetEntitiesDetails(interviewId, ids, sectionId);

        [HttpGet]
        [Route("getFullSectionInfo")]
        public override SectionData GetFullSectionInfo(Guid interviewId, string sectionId) => base.GetFullSectionInfo(interviewId, sectionId);

        [HttpGet]
        [Route("getInterviewStatus")]
        public override InterviewSimpleStatus GetInterviewStatus(Guid interviewId) => base.GetInterviewStatus(interviewId);

        [HttpGet]
        [Route("getNavigationButtonState")]
        public override ButtonState GetNavigationButtonState(Guid interviewId, string sectionId, string id, IQuestionnaire questionnaire = null) =>
            base.GetNavigationButtonState(interviewId, sectionId, id, questionnaire);

        [HttpGet]
        [Route("isEnabled")]
        public override bool IsEnabled(Guid interviewId, string id) => base.IsEnabled(interviewId, id);

        [HttpGet]
        [Route("getPrefilledQuestions")]
        public override InterviewEntityWithType[] GetInterviewEntitiesWithTypes(Guid interviewId) => base.GetInterviewEntitiesWithTypes(interviewId);

        [HttpGet]
        [Route("getPrefilledEntities")]
        public override PrefilledPageData GetPrefilledEntities(Guid interviewId) => base.GetPrefilledEntities(interviewId);

        [HttpGet]
        [Route("getSectionEntities")]
        public override InterviewEntityWithType[] GetSectionEntities(Guid interviewId, string sectionId) => base.GetSectionEntities(interviewId, sectionId);

        [HttpGet]
        [Route("hasCoverPage")]
        public override bool HasCoverPage(Guid interviewId) => base.HasCoverPage(interviewId);

        [HttpGet]
        [Route("getSidebarChildSectionsOf")]
        public override Sidebar GetSidebarChildSectionsOf(Guid interviewId, [FromQuery(Name = "ids[]")] string[] ids, string sectionId = null) => base.GetSidebarChildSectionsOf(interviewId, ids, sectionId);

        [HttpGet]
        [Route("search")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public SearchResults Search(Guid interviewId, [FromQuery(Name="flags[]")] FilterOption[] flags = null, int skip = 0, int limit = 50)
        {
            FilterOption[] flagsEnum = flags ?? new FilterOption[0];
            var interview = GetCallerInterview(interviewId);
            var questionnaire = GetCallerQuestionnaire(interview.QuestionnaireIdentity);
            var result = this.statefullInterviewSearcher.Search(interview, questionnaire, flagsEnum, skip, limit);
            return result;
        }

        [HttpGet]
        [Route("overview")]
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

        [HttpGet]
        [Route("overviewItemAdditionalInfo")]
        public OverviewItemAdditionalInfo OverviewItemAdditionalInfo(Guid interviewId, string id)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            var currentUserId = this.authorizedUser.Id;
            var additionalInfo = this.overviewService.GetOverviewItemAdditionalInfo(statefulInterview, id, currentUserId);
            return additionalInfo;
        }

        [HttpGet]
        [Route("getStatusesHistory")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @filters.js")]
        public List<CommentedStatusHistoryView> GetStatusesHistory(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            return this.changeStatusFactory.GetFilteredStatuses(statefulInterview.Id);
        }

        [HttpGet]
        [Route("getFlags")]
        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Used by HqApp @flags.js")]
        public IEnumerable<string> GetFlags(Guid interviewId)
        {
            var statefulInterview = this.GetCallerInterview(interviewId);
            return this.interviewFactory.GetFlaggedQuestionIds(statefulInterview.Id).Select(x => x.ToString());
        }

        [HttpGet]
        [Route("getInterviewDetails")]
        public override InterviewInfo GetInterviewDetails(Guid interviewId)
        {
            var interviewDetails = base.GetInterviewDetails(interviewId);
            if (IsReviewMode())
            {
                interviewDetails.DoesBrokenPackageExist =
                    this.interviewBrokenPackagesService.IsNeedShowBrokenPackageNotificationForInterview(interviewId);
            }

            return interviewDetails;
        }
    }
}
