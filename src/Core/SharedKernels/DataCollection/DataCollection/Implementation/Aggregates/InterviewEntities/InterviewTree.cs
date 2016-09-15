using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        public InterviewTree(Guid interviewId)
        {
            this.InterviewId = interviewId.FormatGuid();
        }

        public string InterviewId { get; }

        public IList<InterviewTreeSection> Sections { get; } = new List<InterviewTreeSection>();

        public IReadOnlyCollection<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this
                .Sections
                .Cast<IInterviewTreeNode>()
                .TreeToEnumerable(node => node.Children)
                .OfType<InterviewTreeQuestion>()
                .Where(node => node.Identity.Id == questionId)
                .ToReadOnlyCollection();

        public override string ToString()
            => $"Tree ({this.InterviewId})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Sections.Select(section => section.ToString().PrefixEachLine("  ")));
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }

        IList<IInterviewTreeNode> Children { get; }
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode
    {
        protected InterviewTreeLeafNode(Identity identity)
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

        public override string ToString() => $"Question ({this.Identity})";
    }

    public class InterviewTreeStaticText : InterviewTreeLeafNode
    {
        public InterviewTreeStaticText(Identity identity)
            : base(identity) {}

        public override string ToString() => $"Text ({this.Identity})";
    }

    public abstract class InterviewTreeGroup : IInterviewTreeNode
    {
        protected InterviewTreeGroup(Identity identity, IEnumerable<IInterviewTreeNode> children)
        {
            this.Identity = identity;
            this.Children = children.ToList();
        }

        public Identity Identity { get; }
        public IList<IInterviewTreeNode> Children { get; }
    }

    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public InterviewTreeSubSection(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}

        public override string ToString()
            => $"SubSection ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeSection : InterviewTreeGroup
    {
        public InterviewTreeSection(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}

        public override string ToString()
            => $"Section ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeRoster : InterviewTreeGroup
    {
        public InterviewTreeRoster(Identity identity, IEnumerable<IInterviewTreeNode> children)
            : base(identity, children) {}

        public override string ToString()
            => $"Roster ({this.Identity})" + Environment.NewLine
            + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }
}