using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.UI.Headquarters.API.WebInterview.Services.Overview;

namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class InterviewOverviewService : IInterviewOverviewService
    {
        public IEnumerable<OverviewNode> GetOverview(IStatefulInterview interview, IQuestionnaire questionnaire,
            bool isReviewMode)
        {
            var enabledSectionIds = interview.GetEnabledSections().Select(x => x.Identity).ToHashSet();

            foreach (var enabledSectionId in enabledSectionIds)
            {
                var interviewEntities = isReviewMode
                    ? interview.GetUnderlyingEntitiesForReviewRecursive(enabledSectionId)
                    : interview.GetUnderlyingInterviewerEntities(enabledSectionId);
                
                foreach (var interviewEntity in interviewEntities.Where(interview.IsEnabled))
                    yield return BuildOverviewNode(interviewEntity, interview, questionnaire, enabledSectionIds);
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
                return new OverviewItemAdditionalInfo(question, interview, currentUserId);
            }

            var staticText = interview.GetStaticText(identity);
            if (staticText != null)
            {
                return new OverviewItemAdditionalInfo(staticText, interview);
            }

            return null;
        }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity,
            IStatefulInterview interview,
            IQuestionnaire questionnaire,
            ICollection<Identity> sections)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);
            
            if (question != null)
            {
                return new OverviewWebQuestionNode(question, interview);
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewWebStaticTextNode(staticText, interview)
                {
                    Id = staticText.Identity.ToString(),
                    Title = staticText.Title.Text,
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
                    return new OverviewWebGroupNode(roster)
                    {
                        Id = roster.Identity.ToString(),
                        Title = roster.Title.Text,
                        RosterTitle = roster.RosterTitle
                    };
                }

                return new OverviewWebGroupNode(group)
                {
                    Id = group.Identity.ToString(),
                    Title = group.Title.Text
                };
            }

            throw new NotSupportedException($@"Display of {interviewerEntityIdentity} entity is not supported");
        }
    }
}
