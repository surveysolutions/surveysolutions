using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        private readonly IQuestionnaire questionnaire;
        private readonly ISubstitionTextFactory textFactory;

        private Dictionary<Identity, IInterviewTreeNode> nodesCache = new Dictionary<Identity, IInterviewTreeNode>();

        public InterviewTree(Guid interviewId, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            this.InterviewId = interviewId.FormatGuid();
            this.questionnaire = questionnaire;
            this.textFactory = textFactory;
        }

        public void SetSections(IEnumerable<InterviewTreeSection> sections)
        {
            this.Sections = sections.ToList();

            foreach (var section in this.Sections)
            {
                ((IInternalInterviewTreeNode) section).SetTree(this);
            }

            WarmUpCache();
        }

        public string InterviewId { get; }
        public IReadOnlyCollection<InterviewTreeSection> Sections { get; private set; }
        public IEnumerable<IInterviewTreeNode> AllNodes => this.nodesCache.Values;

        public InterviewTreeQuestion GetQuestion(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeQuestion;

        internal InterviewTreeGroup GetGroup(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeGroup;
    
        internal InterviewTreeRoster GetRoster(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeRoster;

        internal InterviewTreeStaticText GetStaticText(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeStaticText;

        public InterviewTreeVariable GetVariable(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeVariable;

        public IEnumerable<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this.FindEntity(questionId).OfType<InterviewTreeQuestion>();

        public IEnumerable<InterviewTreeStaticText> FindStaticTexts()
            => this.nodesCache.Values.OfType<InterviewTreeStaticText>();

        public IEnumerable<InterviewTreeStaticText> FindStaticTexts(Identity parentGroup)
            => this.nodesCache.Values.OfType<InterviewTreeStaticText>()
                .Where(staticText => staticText.Parents.Any(parent => parent.Identity.Equals(parentGroup)));

        public IEnumerable<InterviewTreeQuestion> FindQuestions()
            => this.nodesCache.Values.OfType<InterviewTreeQuestion>();

        public IEnumerable<InterviewTreeQuestion> FindQuestions(Identity parentGroup)
            => this.nodesCache.Values.OfType<InterviewTreeQuestion>()
                .Where(question => question.Parents.Any(parent => parent.Identity.Equals(parentGroup)));

        public IEnumerable<InterviewTreeRoster> FindRosters()
            => this.nodesCache.Values.OfType<InterviewTreeRoster>();

        public IEnumerable<IInterviewTreeNode> FindEntity(Guid nodeId)
            => this.nodesCache.Where(x => x.Key.Id == nodeId).Select(x => x.Value);

        public IInterviewTreeNode GetNodeByIdentity(Identity identity)
        {
            // identity should not be null here, looks suspicious
            if (identity == null) return null;
            return this.nodesCache.GetOrNull(identity);
        }
       
        public void ActualizeTree()
        {
            foreach (var treeSection in this.Sections)
                treeSection.ActualizeChildren();
        }

        public void RemoveNode(Identity identity)
        {
            // should not be null here, looks suspicious
            var parentGroup = this.GetNodeByIdentity(identity)?.Parent as InterviewTreeGroup;
            parentGroup?.RemoveChild(identity);
        }

        public IReadOnlyCollection<InterviewTreeNodeDiff> Compare(InterviewTree that)
        {
            var existingIdentities = this.nodesCache.Keys.Where(thisIdentity => that.nodesCache.ContainsKey(thisIdentity));
            var removedIdentities = this.nodesCache.Keys.Where(thisIdentity => !that.nodesCache.ContainsKey(thisIdentity));
            var addedIdentities = that.nodesCache.Keys.Where(thatIdentity => !this.nodesCache.ContainsKey(thatIdentity));

            var diffs =
                existingIdentities.Select(identity => InterviewTreeNodeDiff.Create(this.nodesCache[identity], that.nodesCache[identity]))
                    .Concat(
                removedIdentities.Select(identity => InterviewTreeNodeDiff.Create(this.nodesCache[identity], null)))
                    .Concat(
                addedIdentities.Select(identity => InterviewTreeNodeDiff.Create(null, that.nodesCache[identity])));

            return diffs
                .Where(diff =>
                    diff.IsNodeAdded ||
                    diff.IsNodeRemoved ||
                    diff.IsNodeDisabled ||
                    diff.IsNodeEnabled ||
                    IsTitleChanged(diff) ||
                    IsRosterTitleChanged(diff as InterviewTreeRosterDiff) ||
                    IsAnswerByQuestionChanged(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionValid(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionInalid(diff as InterviewTreeQuestionDiff) ||
                    IsStaticTextValid(diff as InterviewTreeStaticTextDiff) ||
                    IsStaticTextInalid(diff as InterviewTreeStaticTextDiff) ||
                    IsVariableChanged(diff as InterviewTreeVariableDiff) ||
                    IsOptionsSetChanged(diff as InterviewTreeQuestionDiff) ||
                    IsLinkedToListOptionsSetChanged(diff as InterviewTreeQuestionDiff))
                .ToReadOnlyCollection();
        }

        private bool IsTitleChanged(InterviewTreeNodeDiff interviewTreeNodeDiff)
        {
            if (interviewTreeNodeDiff.IsNodeRemoved)
                return false;

            var diffByQuestion = interviewTreeNodeDiff as InterviewTreeQuestionDiff;
            if (diffByQuestion != null)
            {
                return diffByQuestion.IsTitleChanged || diffByQuestion.AreValidationMessagesChanged;
            }

            var diffByRoster = interviewTreeNodeDiff as InterviewTreeGroupDiff;
            if (diffByRoster != null)
            {
                return diffByRoster.IsTitleChanged;
            }

            var diffByStaticText = interviewTreeNodeDiff as InterviewTreeStaticTextDiff;
            if (diffByStaticText != null)
            {
                return diffByStaticText.IsTitleChanged || diffByStaticText.AreValidationMessagesChanged;
            }

            return false;
        }

        private static bool IsOptionsSetChanged(InterviewTreeQuestionDiff diffByQuestion)
        {
            if (diffByQuestion == null || diffByQuestion.IsNodeRemoved) return false;

            return diffByQuestion.AreLinkedOptionsChanged;
        }

        private static bool IsLinkedToListOptionsSetChanged(InterviewTreeQuestionDiff diffByQuestion)
        {
            if (diffByQuestion == null || diffByQuestion.IsNodeRemoved) return false;

            return diffByQuestion.AreLinkedToListOptionsChanged;
        }

        private static bool IsVariableChanged(InterviewTreeVariableDiff diffByVariable)
            => diffByVariable != null && diffByVariable.IsValueChanged;

        private static bool IsQuestionValid(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsValid;

        private static bool IsQuestionInalid(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsInvalid;

        private static bool IsStaticTextValid(InterviewTreeStaticTextDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsValid;

        private static bool IsStaticTextInalid(InterviewTreeStaticTextDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsInvalid;

        private static bool IsAnswerByQuestionChanged(InterviewTreeQuestionDiff diffByQuestion)
            => diffByQuestion != null && diffByQuestion.IsAnswerChanged;

        private static bool IsRosterTitleChanged(InterviewTreeRosterDiff diffByRoster)
            => diffByRoster != null && diffByRoster.IsRosterTitleChanged;

        //public override string ToString() => $"Tree ({this.InterviewId})";

        public override string ToString()
           => $"Tree ({this.InterviewId})" + Environment.NewLine
           + string.Join(Environment.NewLine, this.Sections.Select(section => section.ToString().PrefixEachLine("  ")));


        public InterviewTree Clone()
        {
            var clonedInterviewTree = (InterviewTree)this.MemberwiseClone();
            clonedInterviewTree.Sections = new List<InterviewTreeSection>();
            var sections = this.Sections.Select(s => (InterviewTreeSection)s.Clone()).ToList();
            clonedInterviewTree.SetSections(sections);
            return clonedInterviewTree;
        }

        public IInterviewTreeNode CreateNode(QuestionnaireReferenceType type, Identity identity)
        {
            switch (type)
            {
                case QuestionnaireReferenceType.SubSection: return CreateSubSection(identity);
                case QuestionnaireReferenceType.StaticText: return CreateStaticText(identity);
                case QuestionnaireReferenceType.Variable: return CreateVariable(identity);
                case QuestionnaireReferenceType.Question: return CreateQuestion(identity);
                case QuestionnaireReferenceType.Roster: throw new ArgumentException("Use roster manager to create rosters");
            }
            return null;
        }

        public InterviewTreeQuestion CreateQuestion(Identity questionIdentity)
        {
            return CreateQuestion(this, this.questionnaire, this.textFactory, questionIdentity);
        }

        public static InterviewTreeQuestion CreateQuestion(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, Identity questionIdentity)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionIdentity.Id);

            SubstitionText title = textFactory.CreateText(questionIdentity, questionnaire.GetQuestionTitle(questionIdentity.Id), questionnaire);

            SubstitionText[] validationMessages = questionnaire.GetValidationMessages(questionIdentity.Id)
                .Select(x => textFactory.CreateText(questionIdentity, x, questionnaire))
                .ToArray();


            string variableName = questionnaire.GetQuestionVariableName(questionIdentity.Id);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionIdentity.Id);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionIdentity.Id);
            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionIdentity.Id);
            Guid? sourceForLinkedQuestion = null;
            Identity commonParentRosterForLinkedQuestion = null;

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionIdentity.Id);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionIdentity.Id);
            var isLinkedToListQuestion = questionnaire.IsLinkedToListQuestion(questionIdentity.Id);
            var isTimestampQuestion = questionnaire.IsTimestampQuestion(questionIdentity.Id);

            if (isLinkedToQuestion)
                sourceForLinkedQuestion = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);

            if (isLinkedToRoster)
                sourceForLinkedQuestion = questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id);

            if (sourceForLinkedQuestion.HasValue)
            {
                Guid? targetRoster = questionnaire.GetCommonParentRosterForLinkedQuestionAndItSource(questionIdentity.Id);

                if (targetRoster.HasValue)
                {
                    var level = isLinkedToRoster  
                        ? questionnaire.GetRosterLevelForGroup(sourceForLinkedQuestion.Value) - 1
                        : questionnaire.GetRosterLevelForEntity(targetRoster.Value) + 1;
                    var commonParentRosterVector = questionIdentity.RosterVector.Take(level).ToArray();
                    commonParentRosterForLinkedQuestion = new Identity(targetRoster.Value, commonParentRosterVector);
                }
            }

            return new InterviewTreeQuestion(questionIdentity,
                title: title, 
                variableName: variableName, 
                questionType: questionType, 
                answer: null,
                linkedOptions: null, 
                cascadingParentQuestionId: cascadingParentQuestionId,
                isYesNo: isYesNoQuestion, 
                isDecimal: isDecimalQuestion, 
                isLinkedToListQuestion: isLinkedToListQuestion, 
                isTimestampQuestion: isTimestampQuestion, 
                linkedSourceId: sourceForLinkedQuestion, 
                commonParentRosterIdForLinkedQuestion: commonParentRosterForLinkedQuestion, 
                validationMessages: validationMessages);
        }

        public static InterviewTreeVariable CreateVariable(Identity variableIdentity)
        {
            return new InterviewTreeVariable(variableIdentity);
        }

        public InterviewTreeSubSection CreateSubSection(Identity subSectionIdentity)
        {
            return CreateSubSection(this, this.questionnaire, textFactory, subSectionIdentity);
        }

        public static InterviewTreeSubSection CreateSubSection(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, Identity subSectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(subSectionIdentity.Id);
            SubstitionText title = textFactory.CreateText(subSectionIdentity, questionnaire.GetGroupTitle(subSectionIdentity.Id), questionnaire);
            return new InterviewTreeSubSection(subSectionIdentity, title, childrenReferences);
        }

        public static InterviewTreeSection CreateSection(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, Identity sectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(sectionIdentity.Id);
            SubstitionText title = textFactory.CreateText(sectionIdentity, questionnaire.GetGroupTitle(sectionIdentity.Id), questionnaire);
            return new InterviewTreeSection(sectionIdentity, title, childrenReferences);
        }

        public InterviewTreeStaticText CreateStaticText(Identity staticTextIdentity)
        {
            return CreateStaticText(this, this.questionnaire, textFactory, staticTextIdentity);
        }

        public static InterviewTreeStaticText CreateStaticText(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, Identity staticTextIdentity)
        {
            SubstitionText title = textFactory.CreateText(staticTextIdentity, questionnaire.GetStaticText(staticTextIdentity.Id), questionnaire);
            SubstitionText[] validationMessages = questionnaire.GetValidationMessages(staticTextIdentity.Id)
                .Select(x => textFactory.CreateText(staticTextIdentity, x, questionnaire))
                .ToArray();
            return new InterviewTreeStaticText(staticTextIdentity, title, validationMessages);
        }

        public RosterManager GetRosterManager(Guid rosterId)
        {
            if (questionnaire.IsFixedRoster(rosterId))
            {
                return new FixedRosterManager(this, this.questionnaire, rosterId, this.textFactory);
            }

            Guid sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            var questionaType = questionnaire.GetQuestionType(sourceQuestionId);
            if (questionaType == QuestionType.MultyOption)
            {
                if (this.questionnaire.IsQuestionYesNo(sourceQuestionId))
                {
                    return new YesNoRosterManager(this, this.questionnaire, rosterId, this.textFactory);
                }
                return new MultiRosterManager(this, this.questionnaire, rosterId, this.textFactory);
            }

            if (questionaType == QuestionType.Numeric)
            {
                return new NumericRosterManager(this, this.questionnaire, rosterId, this.textFactory);
            }

            if (questionaType == QuestionType.TextList)
            {
                return new ListRosterManager(this, this.questionnaire, rosterId, this.textFactory);
            }

            throw new ArgumentException("Unknown roster type");
        }


        private void WarmUpCache()
        {
            this.nodesCache = new Dictionary<Identity, IInterviewTreeNode>();
            foreach (var node in this.Sections.Cast<IInterviewTreeNode>().TreeToEnumerable(node => node.Children))
            {
                nodesCache[node.Identity] = node;
            }
        }
        
        public void ProcessRemovedNodeByIdentity(Identity identity)
        {
            if (!this.nodesCache.ContainsKey(identity)) return;

            var nodesToRemove =
                 this.nodesCache[identity].TreeToEnumerable(node => node.Children)
                    .Select(x => x.Identity)
                    .Union(new[] {identity});

            foreach (var nodeToRemove in nodesToRemove)
            {
                this.nodesCache.Remove(nodeToRemove);
            }
        }

        public void ProcessAddedNode(IInterviewTreeNode node)
        {
            nodesCache[node.Identity] = node;
        }

        public IEnumerable<IInterviewTreeNode> GetEntitiesWithSubstitution()
        {
            return new IInterviewTreeNode[0] {};
        }

        public IInterviewTreeNode FindEntityInQuestionBranch(Guid entityId, Identity questionIdentity)
        {
            for (int i = questionIdentity.RosterVector.Length; i >= 0; i--)
            {
                var entityIdentity = new Identity(entityId, questionIdentity.RosterVector.Take(i).ToArray());
                var entity = this.GetNodeByIdentity(entityIdentity);
                if (entity != null)
                    return entity;
            }

            return null;
        }

        public void ReplaceSubstitutions()
        {
            foreach (var interviewTreeNode in this.nodesCache.Values)
            {
                interviewTreeNode.ReplaceSubstitutions();
            }
        }
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }
        IInterviewTreeNode Parent { get; }
        IEnumerable<IInterviewTreeNode> Parents { get; }
        IReadOnlyCollection<IInterviewTreeNode> Children { get; }

        bool IsDisabled();

        void Disable();
        void Enable();

        IInterviewTreeNode Clone();

        void ReplaceSubstitutions();
    }

    public interface IInternalInterviewTreeNode
    {
        void SetTree(InterviewTree tree);
        void SetParent(IInterviewTreeNode parent);
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;

        protected InterviewTreeLeafNode(Identity identity)
        {
            this.Identity = identity;
            this.isDisabled = false;
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IEnumerable<IInterviewTreeNode> Parents { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        IReadOnlyCollection<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();

        public virtual void SetTree(InterviewTree tree)
        {
            this.Tree = tree;
        }


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

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);

        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public abstract IInterviewTreeNode Clone();

        public abstract void ReplaceSubstitutions();
    }

    public enum QuestionnaireReferenceType
    {
        SubSection = 1,
        Roster = 2,
        StaticText = 10,
        Variable = 20,
        Question = 30,
    }

    [DebuggerDisplay("{ToString()}")]
    public class QuestionnaireItemReference
    {
        public QuestionnaireItemReference(QuestionnaireReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
        }

        public Guid Id { get; set; }

        public QuestionnaireReferenceType Type { get; set; }

        public override string ToString() => $"{Type} {Id.FormatGuid()}";
    }

    public class RosterNodeDescriptor
    {
        public Identity Identity { get; set; }
        public string Title { get; set; }

        public RosterType Type { get; set; }

        public InterviewTreeQuestion SizeQuestion { get; set; }
        public Identity RosterTitleQuestionIdentity { get; set; }
    }
}