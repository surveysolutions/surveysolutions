using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services.Overview;


namespace WB.UI.Headquarters.API.WebInterview.Services
{
    public class InterviewOverviewService : IInterviewOverviewService
    {
        private readonly IWebInterviewInterviewEntityFactory entityFactory;

        public InterviewOverviewService(IWebInterviewInterviewEntityFactory entityFactory)
        {
            this.entityFactory = entityFactory;
        }

        public IEnumerable<OverviewNode> GetOverview(IStatefulInterview interview)
        {
            var interviewEntities = interview.GetUnderlyingInterviewerEntities();
            var sections = interview.GetEnabledSections().Select(x => x.Identity).ToHashSet();
            return interviewEntities.Select(x => BuildOverviewNode(x, interview, sections));
        }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity,
            IStatefulInterview interview,
            ICollection<Identity> sections)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);
            
            if (question != null)
            {
                return new OverviewWebQuestionNode(question);
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewStaticText(staticText)
                {
                    Id = staticText.Identity.ToString(),
                    Title = staticText.Title.Text
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
                        Title = roster.Title.Text  + " - " + roster.RosterTitle,
                        Status = this.entityFactory.CalculateSimpleStatus(roster, true)
                    };
                }

                return new OverviewWebGroupNode(group)
                {
                    Id = group.Identity.ToString(),
                    Status = this.entityFactory.CalculateSimpleStatus(group, true),
                    Title = group.Title.Text
                };
            }

            throw new NotSupportedException($"Display of {interviewerEntityIdentity} entity is not supported");
        }
    }
}
