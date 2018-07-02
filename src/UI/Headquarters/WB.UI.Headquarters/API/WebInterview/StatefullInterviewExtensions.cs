using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.API.WebInterview
{
    public static class StatefullInterviewExtensions
    {
        public static IEnumerable<Link> GetBreadcrumbs(this IInterviewTreeNode nodeParents)
        {
            foreach (var node in nodeParents.Parents)
            {
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

    }
}
