using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public abstract class InterviewTreeGroup : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;
        private List<IInterviewTreeNode> children = new List<IInterviewTreeNode>();

        protected InterviewTreeGroup(Identity identity, SubstitionText title, IEnumerable<QuestionnaireItemReference> childrenReferences)
        {
            this.childEntitiesReferences = childrenReferences.ToReadOnlyCollection();

            this.Identity = identity;
            this.isDisabled = false;
            this.Title = title;

            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode)child).SetParent(this);
            }
        }

        public Identity Identity { get; private set; }
        public SubstitionText Title { get; private set; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        public IEnumerable<IInterviewTreeNode> Parents { get; private set; }

        public IReadOnlyCollection<IInterviewTreeNode> Children => this.children;

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree)
        {
            this.Tree = tree;
            this.Title.SetTree(tree);
            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode) child).SetTree(tree);
            }
        }

        private readonly IReadOnlyCollection<QuestionnaireItemReference> childEntitiesReferences;

        public void ActualizeChildren(bool skipRosters = false)
        {
            foreach (var childEntityReference in this.childEntitiesReferences)
            {
                var childEntityId = childEntityReference.Id;
                switch (childEntityReference.Type)
                {
                    case QuestionnaireReferenceType.Roster:
                        if (skipRosters) continue;
                        this.ActualizeRoster(childEntityId);
                        break;
                    case QuestionnaireReferenceType.SubSection:
                        this.ActualizeGroup(childEntityId);
                        break;
                    case QuestionnaireReferenceType.StaticText:
                    case QuestionnaireReferenceType.Variable:
                    case QuestionnaireReferenceType.Question:
                        var entityIdentity = new Identity(childEntityId, this.RosterVector);
                        var entity = this.Children.FirstOrDefault(x => x.Identity.Equals(entityIdentity));

                        if (entity == null)
                        {
                            entity = this.Tree.CreateNode(childEntityReference.Type, entityIdentity);
                            this.AddChild(entity);
                        }
                        entity.ReplaceSubstitutions();
                        break;
                }
            }
        }

        private void ActualizeGroup(Guid groupId)
        {
            var subSectionIdentity = new Identity(groupId, this.RosterVector);
            var subSection = this.children.OfType<InterviewTreeGroup>().FirstOrDefault(x => x.Identity.Equals(subSectionIdentity));

            if (subSection == null)
            {
                subSection = this.Tree.CreateSubSection(subSectionIdentity);
                this.AddChild(subSection);
            }

            subSection.ActualizeChildren();
            subSection.ReplaceSubstitutions();
        }

        private void ActualizeRoster(Guid rosterId)
        {
            var rosterManager = this.Tree.GetRosterManager(rosterId);

            var expectedRosterIdentities = rosterManager.CalcuateExpectedIdentities(this.Identity);
            var actualRosterIdentities = this.children.Where(x => x.Identity.Id == rosterId).Select(x => x.Identity).ToList();

            var rostersToRemove = actualRosterIdentities.Except(expectedRosterIdentities);
            var rostersToAdd = expectedRosterIdentities.Except(actualRosterIdentities);

            foreach (var rosterToRemove in rostersToRemove)
                this.RemoveChild(rosterToRemove);

            foreach (var rosterToAdd in rostersToAdd)
            {
                var expectedRoster = rosterManager.CreateRoster(this.Identity, rosterToAdd, expectedRosterIdentities.IndexOf(rosterToAdd));

                this.AddChild(expectedRoster);
            }

            foreach (var expectedRoster in expectedRosterIdentities)
            {
                this.children.OfType<InterviewTreeRoster>().Where(x => x.Identity.Equals(expectedRoster)).ForEach(roster =>
                {
                    roster.ActualizeChildren();
                    roster.ReplaceSubstitutions();
                });
            }
        }

        public RosterVector RosterVector => this.Identity.RosterVector;

        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent)
        {
            this.Parent = parent;
            this.Parents = this.GetParents(parent).Reverse();
        }

        private IEnumerable<IInterviewTreeNode> GetParents(IInterviewTreeNode nearestParent)
        {
            while (nearestParent != null)
            {
                yield return nearestParent;
                nearestParent = nearestParent.Parent;
            }
        }

        public void AddChild(IInterviewTreeNode child)
        {
            var internalTreeNode = child as IInternalInterviewTreeNode;
            if (internalTreeNode == null) throw new ArgumentException(nameof(child));

            internalTreeNode.SetTree(this.Tree);
            internalTreeNode.SetParent(this);

            this.children.Add(child);

            Tree?.ProcessAddedNode(child); 
        }

        public void AddRosterAndFixOrder(InterviewTreeRoster childRoster)
        {
            this.AddChild(childRoster);
            this.UpdateSortIndexesForRosters(this.Tree.GetRosterManager(childRoster.Identity.Id));
            this.SortChildrenNodes();
        }

        public void AddChildren(IEnumerable<IInterviewTreeNode> nodes)
        {
            nodes.ForEach(this.AddChild);
        }

        public void RemoveChild(Identity identity)
        {
            var nodeToRemove = this.children.Find(child => child.Identity == identity);
            if (nodeToRemove != null) this.children.Remove(nodeToRemove);

            Tree?.ProcessRemovedNodeByIdentity(identity);
        }

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);
        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public void SetTitle(SubstitionText title) => this.Title = title;

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

        public virtual IInterviewTreeNode Clone()
        {
            var clonedInterviewTreeGroup = (InterviewTreeGroup) this.MemberwiseClone();
            clonedInterviewTreeGroup.Tree = null;
            clonedInterviewTreeGroup.children = new List<IInterviewTreeNode>();
            var clonedChildren = this.Children.Select(n => n.Clone()).ToList();
            clonedInterviewTreeGroup.AddChildren(clonedChildren);
            clonedInterviewTreeGroup.Title = this.Title?.Clone();
            return clonedInterviewTreeGroup;
        }

        public void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions();
        }

        private void UpdateSortIndexesForRosters(RosterManager rosterManager)
        {
            if (rosterManager is MultiRosterManager)
            {
                var orderedIdentities = (rosterManager as MultiRosterManager).GetOrderedExpectedRostersIdentities(this.Identity);
                for (int i = 0; i < orderedIdentities.Count; i++)
                {
                    var roster = this.Tree.GetRoster(orderedIdentities[i]);
                    if (roster != null) roster.SortIndex = i;
                }
            }
            else if (rosterManager is YesNoRosterManager)
            {
                var orderedIdentities = (rosterManager as YesNoRosterManager).GetOrderedExpectedRostersIdentities(this.Identity);
                for (int i = 0; i < orderedIdentities.Count; i++)
                {
                    var roster = this.Tree.GetRoster(orderedIdentities[i]);
                    if (roster != null) roster.SortIndex = i;
                }
            }
        }

        private void SortChildrenNodes()
        {
            var orderedChildren = this.OrderChildrenAsInQuestionnaire().ToList();
            this.children.Clear();
            this.children.AddRange(orderedChildren);
        }

        public IEnumerable<IInterviewTreeNode> OrderChildrenAsInQuestionnaire()
        {
            foreach (var childEntityReference in this.childEntitiesReferences)
            {
                var childEntityId = childEntityReference.Id;
                switch (childEntityReference.Type)
                {
                    case QuestionnaireReferenceType.Roster:
                        var rosters = this.children
                            .Where(x => x.Identity.Id == childEntityId)
                            .OfType<InterviewTreeRoster>()
                            .OrderBy(x => x.SortIndex);
                        foreach (var roster in rosters)
                        {
                            yield return roster;
                        }
                        break;
                    case QuestionnaireReferenceType.SubSection:
                    case QuestionnaireReferenceType.StaticText:
                    case QuestionnaireReferenceType.Variable:
                    case QuestionnaireReferenceType.Question:
                        var group = this.children.FirstOrDefault(x => x.Identity.Id == childEntityId);
                        if (group != null)
                        {
                            yield return group;
                        }
                        break;
                }
            }
        }

        private IEnumerable<InterviewTreeQuestion> GetEnabledInterviewerQuestions()
            => this.Children.TreeToEnumerable(x => x.Children).OfType<InterviewTreeQuestion>()
                .Where(x => !x.IsPrefilled && x.IsInterviewer && !x.IsDisabled());

        private IEnumerable<InterviewTreeStaticText> GetEnabledStaticTexts()
            => this.Children.TreeToEnumerable(x => x.Children).OfType<InterviewTreeStaticText>()
                .Where(x => !x.IsDisabled());

        public int CountEnabledQuestions() => this.GetEnabledInterviewerQuestions().Count();
        public int CountEnabledAnsweredQuestions() => this.GetEnabledInterviewerQuestions().Count(question => question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTexts()
            => this.GetEnabledInterviewerQuestions().Count(question => !question.IsValid) +
               this.GetEnabledStaticTexts().Count(staticText => !staticText.IsValid);

        public bool HasUnansweredQuestions()
            => this.GetEnabledInterviewerQuestions().Any(question => !question.IsAnswered());
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public InterviewTreeSubSection(Identity identity, SubstitionText title, IEnumerable<QuestionnaireItemReference> childrenReferences) : base(identity, title, childrenReferences)
        {
        }

        public override string ToString() => $"SubSection ({this.Identity})" + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
        //public override string ToString() => $"SubSection {this.Identity} '{this.Title}'. " +
        //                                     $" {(this.IsDisabled() ? "Disabled" : "Enabled")}. ";
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSection : InterviewTreeGroup
    {
        public InterviewTreeSection(Identity identity, SubstitionText title, IEnumerable<QuestionnaireItemReference> childrenReferences) : base(identity, title, childrenReferences)
        {
        }

        public override string ToString() => $"Section ({this.Identity})" + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));


        //public override string ToString() => $"Section {this.Identity} '{this.Title}'. " +
        //                                     $" {(this.IsDisabled() ? "Disabled" : "Enabled")}. ";
    }
}