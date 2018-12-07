using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.API.WebInterview
{
    public static class WebStatefullInterviewExtensions
    {
        public static IEnumerable<Link> GetBreadcrumbs(this IInterviewTreeNode nodeParents, IQuestionnaire questionnaire)
        {
            foreach (var node in nodeParents.Parents)
            {
                if (questionnaire.IsPlainMode(node.Identity.Id))
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
            while (parent != null && questionnaire.IsPlainMode(parent.Identity.Id))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        public static Identity GetParentIdentity(this IInterviewTreeNode node, IQuestionnaire questionnaire)
        {
            return GetParent(node, questionnaire)?.Identity;
        }

        public static IEnumerable<Identity> GetGroupIdentities(IStatefulInterview statefulInterview, IQuestionnaire questionnaire, string sectionId, bool IsReviewMode)
        {
            var result = GetGroupEntities(statefulInterview, questionnaire, sectionId, IsReviewMode);
            var ids = result.Select(x => x.Identity);
            return ids;
        }

        public static IEnumerable<IInterviewTreeNode> GetGroupEntities(IStatefulInterview statefulInterview, IQuestionnaire questionnaire, string sectionId, bool IsReviewMode)
        {
            Identity sectionIdentity = Identity.Parse(sectionId);
            if (statefulInterview == null) return null;
            List<IInterviewTreeNode> nodes = new List<IInterviewTreeNode>();

            var groupEntities = statefulInterview.GetGroup(sectionIdentity).Children;

            foreach (var treeNode in groupEntities)
            {
                if (questionnaire.IsPlainMode(treeNode.Identity.Id))
                {
                    nodes.AddRange(treeNode.Children);
                }
                else
                {
                    nodes.Add(treeNode);
                }
            }

            IEnumerable<IInterviewTreeNode> result = nodes;

            if (!IsReviewMode)
            {
                result = result.Except(x =>
                    questionnaire.IsQuestion(x.Identity.Id) && !questionnaire.IsInterviewierQuestion(x.Identity.Id)
                ).ToList();
            }

            return result.Except(x =>
                questionnaire.IsVariable(x.Identity.Id)
            );
        }
    }
}
