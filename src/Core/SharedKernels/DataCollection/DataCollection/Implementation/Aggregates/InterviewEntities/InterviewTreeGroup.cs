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

        protected InterviewTreeGroup(Identity identity,
            SubstitutionText title, 
            IEnumerable<QuestionnaireItemReference> childrenReferences)
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
        public SubstitutionText Title { get; private set; }
        public InterviewTree Tree { get; private set; }

        public IInterviewTreeNode Parent
        {
            get;
            private set;
        }

        public IEnumerable<IInterviewTreeNode> Parents { get; private set; } = Array.Empty<IInterviewTreeNode>();

        public IReadOnlyCollection<IInterviewTreeNode> Children => this.children;

        public void SetChildren(List<IInterviewTreeNode> children) => this.children = children;

        public void SetIdentity(Identity identity) => this.Identity = identity;

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree)
        {
            this.Tree = tree;
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

            var expectedRosterIdentities = rosterManager.CalcuateExpectedIdentities(this.Identity);

            var actualRosterIdentities = this.children
                .Where(x => x.Identity.Id == rosterId)
                .Select(x => x.Identity)
                .ToHashSet();

            HashSet<Identity> rostersToRemove = new HashSet<Identity>(actualRosterIdentities);

            List<Identity> rostersToAdd = new List<Identity>();
            List<Identity> rostersToUpdate = new List<Identity>();
            Dictionary<Identity, int> rosterIndexMap = new Dictionary<Identity, int>();

            for (var index = 0; index < expectedRosterIdentities.Count; index++)
            {
                var expectedRosterIdentity = expectedRosterIdentities[index];
                rosterIndexMap[expectedRosterIdentity] = index;

                if (actualRosterIdentities.Contains(expectedRosterIdentity))
                {
                    rostersToRemove.Remove(expectedRosterIdentity);
                    rostersToUpdate.Add(expectedRosterIdentity);
                }
                else
                {
                    rostersToAdd.Add(expectedRosterIdentity);
                }
            }

            this.RemoveChildren(rostersToRemove.ToList());
            
            if (rostersToAdd.Any() || rostersToUpdate.Any())
            {
                InterviewTreeRoster roster = rosterManager.CreateRoster(rosterId);

                int? baseIndex = null;

                foreach (var rosterToAdd in rostersToAdd)
                {
                    InterviewTreeRoster expectedRoster = (InterviewTreeRoster) roster.Clone();

                    var sortIndex = rosterIndexMap[rosterToAdd]; //expectedRosterIdentities.IndexOf(rosterToAdd);
                    rosterManager.UpdateRoster(expectedRoster, this.Identity, rosterToAdd, sortIndex);

                    baseIndex ??= this.IndexOfFirstRosterInstance(expectedRoster);
                    int indexOfRosterInstance = baseIndex.Value + sortIndex;
                    
                    this.AddOrInsertChild(expectedRoster, indexOfRosterInstance);
                }

                foreach (var rosterToUpdate in rostersToUpdate)
                {
                    var sortIndex = rosterIndexMap[rosterToUpdate];// expectedRosterIdentities.IndexOf(rosterToUpdate);
                    rosterManager.UpdateRoster(this.Tree.GetRoster(rosterToUpdate), this.Identity, rosterToUpdate, sortIndex);
                }
            }

            var expectedRosters = this.children
                .Where(roster => roster.Identity.Id == rosterId);

            foreach (var expectedRoster in expectedRosters)
            {
                var roster = expectedRoster as InterviewTreeRoster;
                roster?.ActualizeChildren();
                roster?.ReplaceSubstitutions();
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
            int index = 0;

            foreach (IInterviewTreeNode child in this.children)
            {
                if (child is InterviewTreeRoster treeRoster)
                {
                    if (treeRoster.Identity.Id == roster.Identity.Id)
                    {
                        return index;
                    }
                }

                index++;
            }

            return this.IndexOfExpectedFirstRosterInstance(roster.Identity.Id);
        }

        private int IndexOfExpectedFirstRosterInstance(Guid rosterId)
        {
            int indexOfRosterInQuestionnaireGroup = 0;

            foreach (var itemReference in childEntitiesReferences)
            {
                if (itemReference.Id == rosterId) break;
                indexOfRosterInQuestionnaireGroup++;
            }
            
            var prevRosters = this.childEntitiesReferences
                .Where((reference, index) =>
                index < indexOfRosterInQuestionnaireGroup && reference.Type == QuestionnaireReferenceType.Roster)
                .Select(x => x.Id)
                .ToHashSet();

            var prevRosterInstances = this.children.Count(x => prevRosters.Contains(x.Identity.Id));

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

        public void RemoveChildren(List<Identity> identities)
        {
            var ids = identities.ToHashSet();
            children.RemoveAll(child => ids.Contains(child.Identity));

            foreach (var id in identities)
            {
                Tree?.ProcessRemovedNodeByIdentity(id);
            }
        }

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);
        public bool IsDisabledByOwnCondition() => this.isDisabled;
        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public void SetTitle(SubstitutionText title)
        {
            this.Title = title;
        }

        public InterviewTreeQuestion GetQuestionFromThisOrUpperLevel(Guid questionId)
        {
            for (int i = this.Identity.RosterVector.Length; i >= 0; i--)
            {
                var questionIdentity = new Identity(questionId, this.Identity.RosterVector.Take(i));
                var question = this.Tree.GetQuestion(questionIdentity);
                if (question != null)
                    return question;
            }

            return null;
        }
        
        public InterviewTreeVariable GetVariableFromThisOrUpperLevel(Guid attachedVariableId)
        {
            for (int i = this.Identity.RosterVector.Length; i >= 0; i--)
            {
                var variableIdentity = new Identity(attachedVariableId, this.Identity.RosterVector.Take(i));
                var variable = this.Tree.GetVariable(variableIdentity);
                if (variable != null)
                    return variable;
            }

            return null;
        }

        public virtual IInterviewTreeNode Clone()
        {
            var clonedInterviewTreeGroup = (InterviewTreeGroup) this.MemberwiseClone();
            clonedInterviewTreeGroup.Tree = null;
            clonedInterviewTreeGroup.children = new List<IInterviewTreeNode>();
            var clonedChildren = this.Children.Select(n => n.Clone());
            clonedInterviewTreeGroup.AddChildren(clonedChildren);
            clonedInterviewTreeGroup.Title = this.Title?.Clone();
            return clonedInterviewTreeGroup;
        }

        public virtual void Accept(IInterviewTreeUpdater updater)
        {
            updater.UpdateEnablement(this); 
        }

        public abstract NodeType NodeType { get; }

        public void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions(this.Tree);
        }

        private IEnumerable<InterviewTreeQuestion> GetEnabledInterviewerQuestions()
        {
            if (!this.Tree.Questionnaire.IsCoverPageSupported)
                return this.Children.OfType<InterviewTreeQuestion>()
                    .Where(x => !x.IsPrefilled && x.IsInterviewer && !x.IsDisabled());
            
            return this.Children.OfType<InterviewTreeQuestion>()
                .Where(x => x.IsInterviewer && !x.IsDisabled() && !x.IsReadonly);
        }

        private IEnumerable<InterviewTreeStaticText> GetEnabledStaticTexts()
            => this.Children.OfType<InterviewTreeStaticText>()
                .Where(x => !x.IsDisabled());

        private IEnumerable<InterviewTreeQuestion> GetAllNestedEnabledInterviewerQuestions()
        {
            if (!this.Tree.Questionnaire.IsCoverPageSupported)
                return this.TreeToEnumerableDepthFirst<IInterviewTreeNode>(s => s.Children)
                    .OfType<InterviewTreeQuestion>()
                    .Where(x => !x.IsPrefilled && x.IsInterviewer && !x.IsDisabled());

            return this.TreeToEnumerableDepthFirst<IInterviewTreeNode>(s => s.Children)
                .OfType<InterviewTreeQuestion>()
                .Where(x => x.IsInterviewer && !x.IsDisabled() && !x.IsReadonly);
        }

        public int CountNestedEnabledQuestions() => this.GetAllNestedEnabledInterviewerQuestions().Count();

        public int CountNestedEnabledAnsweredQuestions() => this.GetAllNestedEnabledInterviewerQuestions().Count(question => question.IsAnswered());

        public int CountEnabledQuestions() => this.GetEnabledInterviewerQuestions().Count();
        public int CountEnabledAnsweredQuestions() => this.GetEnabledInterviewerQuestions().Count(question => question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTexts()
            => this.GetEnabledInterviewerQuestions().Count(question => !question.IsValid) +
               this.GetEnabledStaticTexts().Count(staticText => !staticText.IsValid);

        public bool HasUnansweredQuestions()
            => this.GetEnabledInterviewerQuestions().Any(question => !question.IsAnswered());

        private IEnumerable<InterviewTreeQuestion> GetEnabledQuestionsForSupervisor()
            => this.Children.OfType<InterviewTreeQuestion>()
                .Where(x => !x.IsDisabled());
        
        public int CountEnabledAnsweredQuestionsForSupervisor() 
            => this.GetEnabledQuestionsForSupervisor().Count(question => question.IsAnswered());

        public int CountEnabledInvalidQuestionsAndStaticTextsForSupervisor()
            => this.GetEnabledQuestionsForSupervisor().Count(question => !question.IsValid) +
               this.GetEnabledStaticTexts().Count(staticText => !staticText.IsValid);

        public bool HasUnansweredQuestionsForSupervisor()
            => this.GetEnabledQuestionsForSupervisor().Any(question => !question.IsAnswered());

        public IEnumerable<Identity> GetEnabledSubGroups()
            => this.Children.OfType<InterviewTreeGroup>()
                .Where(group => !group.IsDisabled())
                .Select(group => group.Identity);
        
        public IEnumerable<InterviewTreeGroup> GetAllSubGroups()
            => this.Children.OfType<InterviewTreeGroup>();

        public List<Identity> DisableChildNodes()
        {
            var disabledNodes = new List<Identity>();
            foreach (var child in children)
            {
                child.Disable();
                disabledNodes.Add(child.Identity);

                disabledNodes.AddRange((child as InterviewTreeGroup)?.DisableChildNodes() ?? new List<Identity>());
            }

            return disabledNodes;
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSubSection : InterviewTreeGroup
    {
        public override NodeType NodeType => NodeType.Group;
        
        public InterviewTreeSubSection(Identity identity, SubstitutionText title, IEnumerable<QuestionnaireItemReference> childrenReferences) : base(identity, title, childrenReferences)
        {
        }

        public override string ToString() => $"SubSection ({this.Identity}) " + (this.IsDisabled() ? "Disabled" : "Enabled") + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }

    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeSection : InterviewTreeGroup
    {
        public override NodeType NodeType => NodeType.Group;
        
        public InterviewTreeSection(Identity identity, SubstitutionText title, IEnumerable<QuestionnaireItemReference> childrenReferences) : base(identity, title, childrenReferences)
        {
        }

        public override string ToString() => $"Section ({this.Identity}) "+ (this.IsDisabled() ? "Disabled" : "Enabled") + Environment.NewLine + string.Join(Environment.NewLine, this.Children.Select(child => child.ToString().PrefixEachLine("  ")));
    }
}
