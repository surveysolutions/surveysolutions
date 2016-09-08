using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        public IList<InterviewTreeSection> Sections { get; } = new List<InterviewTreeSection>();
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }

        IList<IInterviewTreeNode> Children { get; }
    }

    public class InterviewTreeLeafNode : IInterviewTreeNode
    {
        public InterviewTreeLeafNode(Identity identity)
        {
            this.Identity = identity;
        }

        public Identity Identity { get; }
        IList<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();
    }

    public class InterviewTreeQuestion : InterviewTreeLeafNode
    {
        public InterviewTreeQuestion(Identity identity)
            : base(identity) {}
    }

    public class InterviewTreeStaticText : InterviewTreeLeafNode
    {
        public InterviewTreeStaticText(Identity identity)
            : base(identity) {}
    }

    public class InterviewTreeNodeContainer : IInterviewTreeNode
    {
        public InterviewTreeNodeContainer(Identity identity, IEnumerable<IInterviewTreeNode> children)
        {
            this.Identity = identity;
            this.Children = children.ToList();
        }

        public Identity Identity { get; }
        public IList<IInterviewTreeNode> Children { get; }
    }

    public class InterviewTreeGroup : InterviewTreeNodeContainer
    {
        public InterviewTreeGroup(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}
    }

    public class InterviewTreeSection : InterviewTreeNodeContainer
    {
        public InterviewTreeSection(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}
    }

    public class InterviewTreeRoster : InterviewTreeNodeContainer
    {
        public InterviewTreeRoster(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}
    }
}