using System;
using Microsoft.AspNetCore.Mvc;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;

namespace WB.UI.WebTester.Controllers
{
    [ResponseCache(NoStore = true)]
    [Route("api/webinterview")]
    public class InterviewDataController : Enumerator.Native.WebInterview.Controllers.InterviewDataController
    {
        public InterviewDataController(IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository statefulInterviewRepository, 
            IWebInterviewNotificationService webInterviewNotificationService, IWebInterviewInterviewEntityFactory interviewEntityFactory) 
            : base(questionnaireRepository, statefulInterviewRepository, webInterviewNotificationService, interviewEntityFactory)
        {
        }

        [HttpGet]
        [Route("getLanguageInfo")]
        public override LanguageInfo GetLanguageInfo(Guid interviewId) => base.GetLanguageInfo(interviewId);

        [HttpGet]
        [Route("getTopFilteredOptionsForQuestion")]
        public DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count)
            => base.GetTopFilteredOptionsForQuestion(interviewId, id, filter, count);

        [HttpGet]
        [Route("getTopFilteredOptionsForQuestionWithExclude")]
        public override DropdownItem[] GetTopFilteredOptionsForQuestion(Guid interviewId, string id, string filter, int count,
            [FromQuery(Name = "excludedOptionIds[]")] int[] excludedOptionIds)
            => base.GetTopFilteredOptionsForQuestion(interviewId, id, filter, count, excludedOptionIds);

        [HttpGet]
        [Route("getBreadcrumbs")]
        public override BreadcrumbInfo GetBreadcrumbs(Guid interviewId, string? sectionId = null) 
            => base.GetBreadcrumbs(interviewId, sectionId);

        [HttpGet]
        [Route("getCompleteInfo")]
        public override CompleteInfo GetCompleteInfo(Guid interviewId) => base.GetCompleteInfo(interviewId);

        [HttpGet]
        [Route("getCoverInfo")]
        public override CoverInfo GetCoverInfo(Guid interviewId) => base.GetCoverInfo(interviewId);

        [HttpGet]
        [Route("getEntitiesDetails")]
        public override InterviewEntity[] GetEntitiesDetails(Guid interviewId, [FromQuery(Name = "ids[]")] string[] ids, string? sectionId = null) => base.GetEntitiesDetails(interviewId, ids, sectionId);

        [HttpGet]
        [Route("getFullSectionInfo")]
        public override SectionData GetFullSectionInfo(Guid interviewId, string sectionId) => base.GetFullSectionInfo(interviewId, sectionId);

        [HttpGet]
        [Route("getInterviewStatus")]
        public override InterviewSimpleStatus GetInterviewStatus(Guid interviewId) => base.GetInterviewStatus(interviewId);

        [HttpGet]
        [Route("getNavigationButtonState")]
        public override ButtonState GetNavigationButtonState(Guid interviewId, string sectionId, string id, IQuestionnaire? questionnaire = null) =>
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
        public override Sidebar GetSidebarChildSectionsOf(Guid interviewId, [FromQuery(Name = "ids[]")] string[] ids, string? sectionId = null) 
            => base.GetSidebarChildSectionsOf(interviewId, ids, sectionId);

        [HttpGet]
        [Route("getInterviewDetails")]
        public override InterviewInfo GetInterviewDetails(Guid interviewId) => base.GetInterviewDetails(interviewId);
    }
}
