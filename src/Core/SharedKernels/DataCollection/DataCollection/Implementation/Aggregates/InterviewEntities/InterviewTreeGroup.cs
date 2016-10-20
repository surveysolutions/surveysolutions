using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public abstract class InterviewTreeGroup : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;
        private List<IInterviewTreeNode> children;

        protected InterviewTreeGroup(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled)
        {
            this.childEntitiesReferences = new List<QuestionnaireItemReference>().ToReadOnlyCollection();

            this.Identity = identity;
            this.children = children.ToList();
            this.isDisabled = isDisabled;

            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode)child).SetParent(this);
            }
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        public IReadOnlyCollection<IInterviewTreeNode> Children => this.children;

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree)
        {
            this.Tree = tree;

            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode)child).SetTree(tree);
            }
        }

        private readonly IReadOnlyCollection<QuestionnaireItemReference> childEntitiesReferences;

        public void ActualizeChildren()
        {
            foreach (var childEntityReference in childEntitiesReferences)
            {
                var childEntityId = childEntityReference.Id;
                switch (childEntityReference.Type)
                {
                    case QuestionnaireReferenceType.Roster:
                        var rosterId = childEntityId;
                        var rosterManager = Tree.GetRosterManager(rosterId);
                        List<Identity> expectedRosterIdentities = rosterManager.CalcuateExpectedIdentities(this.Identity);
                        var actualRosterIdentities = GetActualIdentitiesById(rosterId);

                        foreach (var actualRosterIdentity in actualRosterIdentities)
                        {
                            if (expectedRosterIdentities.Contains(actualRosterIdentity))
                                continue;
                            this.RemoveChild(actualRosterIdentity);
                        }

                        for (int index = 0; index < expectedRosterIdentities.Count; index++)
                        {
                            var expectedRosterIdentity = expectedRosterIdentities[index];
                            if (actualRosterIdentities.Contains(expectedRosterIdentity))
                                continue;
                            this.AddChild(rosterManager.CreateRoster(this.Identity, expectedRosterIdentity, index));
                        }

                        break;
                    case QuestionnaireReferenceType.StaticText:
                    case QuestionnaireReferenceType.SubSection:
                    case QuestionnaireReferenceType.Variable:
                    case QuestionnaireReferenceType.Question:
                        var entityIdentity = new Identity(childEntityId, this.RosterVector);
                        if (!HasChild(entityIdentity))
                        {
                            this.AddChild(Tree.CreateNode(childEntityReference.Type, entityIdentity));
                        }
                        break;
                }
            }
        }

        private List<Identity> GetActualIdentitiesById(Guid rosterId)
        {
            return this.Children.Where(x => x.Identity.Id == rosterId).Select(x => x.Identity).ToList();
        }

        public RosterVector RosterVector => this.Identity.RosterVector;

        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent) => this.Parent = parent;

        public void AddChild(IInterviewTreeNode child)
        {
            var internalTreeNode = child as IInternalInterviewTreeNode;
            if (internalTreeNode == null) throw new ArgumentException(nameof(child));

            internalTreeNode.SetTree(this.Tree);
            internalTreeNode.SetParent(this);
            this.children.Add(child);
        }

        public void AddChildren(List<IInterviewTreeNode> nodes)
        {
            nodes.ForEach(this.AddChild);
        }

        public void RemoveChild(Identity identity)
        {
            var nodesToRemove = this.children.Where(x => x.Identity.Equals(identity)).ToArray();
            nodesToRemove.ForEach(nodeToRemove => this.children.Remove(nodeToRemove));
        }

        public void RemoveChildren(Identity[] identities)
        {
            foreach (var child in this.children.Where(x => identities.Contains(x.Identity)))
                this.children.Remove(child);
        }

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);
        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public InterviewTreeQuestion GetQuestionFromThisOrUpperLevel(Guid questionId)
        {
            for (int i = this.Identity.RosterVector.Length; i >= 0; i--)
            {
                var questionIdentity = new Identity(questionId, this.Identity.RosterVector.Take(i).ToArray());
                var question = this.Tree.GetQuestion(questionIdentity);
                if (question != null)
                    return question;
            }

            return null;
        }

        public bool HasChild(Identity identity)
        {
            return this.Children.Any(x => x.Identity.Equals(identity));
        }

        public IInterviewTreeNode Clone()
        {
            var clonedInterviewTreeGroup = (InterviewTreeGroup) this.MemberwiseClone();
            clonedInterviewTreeGroup.children = this.Children.Select(n =>
            {
                var interviewTreeNode = n.Clone();
                ((IInternalInterviewTreeNode) interviewTreeNode).SetParent(clonedInterviewTreeGroup);
                return interviewTreeNode;
            }).ToList();
            return clonedInterviewTreeGroup;
        }
    }

    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public InterviewTreeSubSection(Identity identity) : this (identity, Enumerable.Empty<IInterviewTreeNode>(), false)
        {
        }

        public InterviewTreeSubSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled) : base(identity, children, isDisabled)
        {
        }

        public override string ToString() => $"SubSection ({this.Identity})" + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    public class InterviewTreeSection : InterviewTreeGroup
    {
        public InterviewTreeSection(Identity identity, IEnumerable<IInterviewTreeNode> children, bool isDisabled) : base(identity, children, isDisabled)
        {
        }

        public override string ToString() => $"Section ({this.Identity})" + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }
}