using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.API.WebInterview
{
    public static class WebStatefulInterviewExtensions
    {
        public static IEnumerable<Link> GetBreadcrumbs(this IInterviewTreeNode nodeParents, IQuestionnaire questionnaire)
        {
            foreach (var node in nodeParents.Parents)
            {
                if (questionnaire.IsFlatRoster(node.Identity.Id))
                    continue;

                var link = new Link { Target = node.Identity.ToString() };

                if (node is InterviewTreeSection section)
                {
                    link.Title = section.Title.ToString();
                    yield return link;
                    continue;
                }

                if (node is InterviewTreeRoster roster)
                {
                    link.Title = $@"{roster.Title} - {roster.RosterTitle}";
                    yield return link;
                    continue;
                }

                if (node is InterviewTreeGroup group)
                {
                    link.Title = @group.Title.ToString();
                    yield return link;
                    continue;
                }
            }
        }

        public static IInterviewTreeNode GetParent(this IInterviewTreeNode node, IQuestionnaire questionnaire)
        {
            var parent = node.Parent;
            while (parent != null && questionnaire.IsFlatRoster(parent.Identity.Id))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        public static Identity GetParentIdentity(this IInterviewTreeNode node, IQuestionnaire questionnaire)
        {
            return GetParent(node, questionnaire)?.Identity;
        }
    }
}
