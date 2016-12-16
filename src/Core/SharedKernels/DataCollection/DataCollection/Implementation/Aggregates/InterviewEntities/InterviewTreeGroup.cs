using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public abstract class InterviewTreeGroup : IInterviewTreeNode, IInternalInterviewTreeNode, ISubstitutable
    {
        private bool isDisabled;
        private List<IInterviewTreeNode> children = new List<IInterviewTreeNode>();

        protected InterviewTreeGroup(Identity identity, SubstitionText title, IEnumerable<QuestionnaireItemReference> childrenReferences)
        {
            this.childEntitiesReferences = childrenReferences.ToList();

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

        public void SetChildren(List<IInterviewTreeNode> children) => this.children = children;

        public void SetIdentity(Identity identity) => this.Identity = identity;

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree)
        {
            this.Tree = tree;
            this.Title.SetTree(tree);
            foreach (var child in this.Children)
            {
                ((IInternalInterviewTreeNode) child).SetTree(tree);
            }
        }

        private readonly List<QuestionnaireItemReference> childEntitiesReferences;

        public void ActualizeChildren()
        {
            foreach (var childEntityReference in this.childEntitiesReferences)
            {
                var childEntityId = childEntityReference.Id;

                if (childEntityReference.Type == QuestionnaireReferenceType.Roster)
                {
                    this.ActualizeRoster(childEntityId);
                    continue;
                }
                
                var entityIdentity = Identity.Create(childEntityId, this.RosterVector);
                var entity = this.Tree.GetNodeByIdentity(entityIdentity);
                if (entity == null)
                {
                    entity = this.Tree.CreateNode(childEntityReference.Type, entityIdentity);
                    this.AddChild(entity);
                }
                
                (entity as InterviewTreeGroup)?.ActualizeChildren();
                (entity as ISubstitutable)?.ReplaceSubstitutions();
            }
        }

        private void ActualizeRoster(Guid rosterId)
        {

            RosterManager rosterManager = this.Tree.GetRosterManager(rosterId);

            List<Identity> expectedRosterIdentities = rosterManager.CalcuateExpectedIdentities(this.Identity);
            List<Identity> actualRosterIdentities =
                this.children.Where(x => x.Identity.Id == rosterId).Select(x => x.Identity).ToList();

            var rostersToRemove = actualRosterIdentities.Except(expectedRosterIdentities);
            var rostersToAdd = expectedRosterIdentities.Except(actualRosterIdentities).ToList();

            foreach (var rosterToRemove in rostersToRemove)
                this.RemoveChild(rosterToRemove);

            if (rostersToAdd.Any())
            {

                InterviewTreeRoster roster = rosterManager.CreateRoster(rosterId);

                foreach (var rosterToAdd in rostersToAdd)
                {
                    InterviewTreeRoster expectedRoster = (InterviewTreeRoster) roster.Clone();

                    var sortIndex = expectedRosterIdentities.IndexOf(rosterToAdd);
                    rosterManager.UpdateRoster(expectedRoster, this.Identity, rosterToAdd, sortIndex);


                    int indexOfRosterInstance = this.IndexOfFirstRosterInstance(expectedRoster) + sortIndex;

                    this.AddOrInsertChild(expectedRoster, indexOfRosterInstance);

                }
            }

            var expectedRosters = this.children
                .OfType<InterviewTreeRoster>()
                .Where(roster => roster.Identity.Id == rosterId);

            foreach (var expectedRoster in expectedRosters)
            {
                expectedRoster.ActualizeChildren();
                expectedRoster.ReplaceSubstitutions();
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

        public void AddOrInsertChild(IInterviewTreeNode child, int? insertTo = null)
        {
            var internalTreeNode = child as IInternalInterviewTreeNode;
            if (internalTreeNode == null) throw new ArgumentException(nameof(child));

            internalTreeNode.SetTree(this.Tree);
            internalTreeNode.SetParent(this);

            if (insertTo.HasValue)
                this.children.Insert(insertTo.Value, child);
            else
                this.children.Add(child);

            Tree?.ProcessAddedNode(child);
        }

        public void AddChild(IInterviewTreeNode child) => this.AddOrInsertChild(child);

        private int IndexOfFirstRosterInstance(InterviewTreeRoster roster)
        {
            var firstRosterInstance = this.children
                .OfType<InterviewTreeRoster>()
                .FirstOrDefault(x => x.Identity.Id == roster.Identity.Id);

            return firstRosterInstance == null
                ? this.IndexOfExpectedFirstRosterInstance(roster.Identity.Id)
                : this.children.IndexOf(firstRosterInstance);
        }

        private int IndexOfExpectedFirstRosterInstance(Guid rosterId)
        {
            var rosterQuestionnaireReference = this.childEntitiesReferences.Find(x => x.Id == rosterId);
            var indexOfRosterInQuestionnaireGroup = this.childEntitiesReferences.IndexOf(rosterQuestionnaireReference);

            var prevRosters = this.childEntitiesReferences.Where((reference, index) =>
                index < indexOfRosterInQuestionnaireGroup && reference.Type == QuestionnaireReferenceType.Roster)
                .Select(x => x.Id)
                .ToList();

            var prevRosterInstances = this.children.Select(x => x.Identity.Id).Count(x => prevRosters.Contains(x));

            return indexOfRosterInQuestionnaireGroup - prevRosters.Count + prevRosterInstances;
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

        public void SetTitle(SubstitionText title)
        {
            this.Title = title;
            this.Title.SetTree(this.Tree);
        }

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

        private IEnumerable<InterviewTreeQuestion> GetEnabledInterviewerQuestions()
            => this.Children.OfType<InterviewTreeQuestion>()
                .Where(x => !x.IsPrefilled && x.IsInterviewer && !x.IsDisabled());

        private IEnumerable<InterviewTreeStaticText> GetEnabledStaticTexts()
            => this.Children.OfType<InterviewTreeStaticText>()
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