using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        public List<InterviewTreeSection> Sections { get; } = new List<InterviewTreeSection>();
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }

        IList<IInterviewTreeNode> Children { get; }
    }

    public class InterviewTreeQuestion : IInterviewTreeNode
    {
        public InterviewTreeQuestion(Identity identity)
        {
            this.Identity = identity;
        }

        public Identity Identity { get; }
        IList<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();
    }

    public class InterviewTreeNodeContainer : IInterviewTreeNode
    {
        public InterviewTreeNodeContainer(Identity identity)
        {
            this.Identity = identity;
        }

        public Identity Identity { get; }
        public IList<IInterviewTreeNode> Children { get; } = new List<IInterviewTreeNode>();
    }

    public class InterviewTreeGroup : InterviewTreeNodeContainer
    {
        public InterviewTreeGroup(Identity identity)
            : base(identity) {}
    }

    public class InterviewTreeSection : InterviewTreeNodeContainer
    {
        public InterviewTreeSection(Identity identity)
            : base(identity) {}
    }

    public class InterviewTreeRoster : InterviewTreeNodeContainer
    {
        public InterviewTreeRoster(Identity identity)
            : base(identity) {}
    }
}