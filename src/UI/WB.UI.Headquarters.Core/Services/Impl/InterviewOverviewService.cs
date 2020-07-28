using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services.Overview;

namespace WB.UI.Headquarters.Services.Impl
{
    public class InterviewOverviewService : IInterviewOverviewService
    {
        private readonly IWebInterviewInterviewEntityFactory interviewEntityFactory;
        private readonly IWebNavigationService webNavigationService;
        private readonly IAuthorizedUser authorizedUser;

        public InterviewOverviewService(IWebInterviewInterviewEntityFactory interviewEntityFactory,
            IWebNavigationService webNavigationService,
            IAuthorizedUser authorizedUser)
        {
            this.interviewEntityFactory = interviewEntityFactory;
            this.webNavigationService = webNavigationService;
            this.authorizedUser = authorizedUser;
        }

        public IEnumerable<OverviewNode> GetOverview(IStatefulInterview interview, IQuestionnaire questionnaire,
            bool isReviewMode)
        {
            var currentUserId = authorizedUser.Id;
            var enabledSectionIds = Enumerable.ToHashSet(interview.GetEnabledSections().Select(x => x.Identity));

            foreach (var enabledSectionId in enabledSectionIds)
            {
                var interviewEntities = isReviewMode
                    ? interview.GetUnderlyingEntitiesForReviewRecursive(enabledSectionId)
                    : interview.GetUnderlyingInterviewerEntities(enabledSectionId);
                
                foreach (var interviewEntity in interviewEntities.Where(interview.IsEnabled))
                    yield return BuildOverviewNode(interviewEntity, interview, questionnaire, enabledSectionIds, currentUserId);
            }
        }

        public OverviewItemAdditionalInfo GetOverviewItemAdditionalInfo(IStatefulInterview interview, string entityId, Guid currentUserId)
        {
            if (!Identity.TryParse(entityId, out Identity identity))
            {
                return null;
            }

            var question = interview.GetQuestion(identity);
            if (question != null)
            {
                var additionalInfo = new OverviewItemAdditionalInfo(question, interview, currentUserId);
                additionalInfo.Errors = additionalInfo.Errors.Select(this.webNavigationService.ResetNavigationLinksToDefault).ToArray();
                additionalInfo.Warnings = additionalInfo.Warnings.Select(this.webNavigationService.ResetNavigationLinksToDefault).ToArray();

                return additionalInfo;
            }

            var staticText = interview.GetStaticText(identity);
            if (staticText != null)
            {
                var additionalInfo = new OverviewItemAdditionalInfo(staticText, interview);
                additionalInfo.Errors = additionalInfo.Errors.Select(this.webNavigationService.ResetNavigationLinksToDefault).ToArray();
                additionalInfo.Warnings = additionalInfo.Warnings.Select(this.webNavigationService.ResetNavigationLinksToDefault).ToArray();

                return additionalInfo;
            }

            return null;
        }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity,
            IStatefulInterview interview,
            IQuestionnaire questionnaire,
            ICollection<Identity> sections,
            Guid currentUserId)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);
            
            if (question != null)
            {
                var overviewQuestion = new OverviewWebQuestionNode(question, interview, currentUserId);
                overviewQuestion.Title = this.webNavigationService.ResetNavigationLinksToDefault(overviewQuestion.Title);
                return overviewQuestion;
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewWebStaticTextNode(staticText, interview)
                {
                    Id = staticText.Identity.ToString(),
                    Title = this.webNavigationService.ResetNavigationLinksToDefault(staticText.Title.Text),
                    AttachmentContentId = questionnaire.GetAttachmentForEntity(staticText.Identity.Id)?.ContentId
                };
            }

            var group = interview.GetGroup(interviewerEntityIdentity);
            if (group != null)
            {
                if (sections.Contains(group.Identity))
                {
                    return new OverviewWebSectionNode(group)
                    {
                        Id = group.Identity.ToString(),
                        Title = group.Title.Text
                    };
                }

                if (group is InterviewTreeRoster roster)
                {
                    return new OverviewWebGroupNode(roster, questionnaire)
                    {
                        Id = roster.Identity.ToString(),
                        Title = roster.Title.Text,
                        RosterTitle = roster.RosterTitle
                    };
                }

                return new OverviewWebGroupNode(group, questionnaire)
                {
                    Id = group.Identity.ToString(),
                    Title = group.Title.Text
                };
            }

            throw new NotSupportedException($@"Display of {interviewerEntityIdentity} entity is not supported");
        }
    }
}
