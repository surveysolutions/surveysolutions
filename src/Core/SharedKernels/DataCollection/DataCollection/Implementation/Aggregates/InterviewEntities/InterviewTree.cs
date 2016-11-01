using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTree
    {
        private readonly IQuestionnaire questionnaire;

        private Dictionary<Identity, IInterviewTreeNode> nodesCache = new Dictionary<Identity, IInterviewTreeNode>();

        public InterviewTree(Guid interviewId, IQuestionnaire questionnaire, IEnumerable<InterviewTreeSection> sections)
        {
            this.InterviewId = interviewId.FormatGuid();
            this.questionnaire = questionnaire;

            this.SetSections(sections);
        }

        private void SetSections(IEnumerable<InterviewTreeSection> sections)
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
        
        public InterviewTreeQuestion GetQuestion(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeQuestion;

        internal InterviewTreeGroup GetGroup(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeGroup;

        internal InterviewTreeStaticText GetStaticText(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeStaticText;

        public InterviewTreeVariable GetVariable(Identity identity)
            => this.GetNodeByIdentity(identity) as InterviewTreeVariable;

        public IEnumerable<InterviewTreeQuestion> FindQuestions(Guid questionId)
            => this.FindEntity(questionId).OfType<InterviewTreeQuestion>();

        public IEnumerable<InterviewTreeQuestion> FindQuestions()
            => this.nodesCache.Values.OfType<InterviewTreeQuestion>();

        public IEnumerable<InterviewTreeRoster> FindRosters()
            => this.nodesCache.Values.OfType<InterviewTreeRoster>();

        public IEnumerable<IInterviewTreeNode> FindEntity(Guid nodeId)
        {
            return this.nodesCache.Where(x => x.Key.Id == nodeId).Select(x => x.Value);
        }

        private IInterviewTreeNode GetNodeByIdentity(Identity identity)
        {
            // identity should not be null here, looks suspicious
            if (identity == null) return null;
            return this.nodesCache.GetOrNull(identity);
        }
       

        public void ActualizeTree()
        {
            var itemsQueue = new Queue<IInterviewTreeNode>(Sections);

            while (itemsQueue.Count > 0)
            {
                var currentItem = itemsQueue.Dequeue();
                if (!(currentItem is InterviewTreeGroup))
                    continue;
                var currentGroup = currentItem as InterviewTreeGroup;
                currentGroup.ActualizeChildren();

                foreach (var childItem in currentGroup.Children)
                {
                    itemsQueue.Enqueue(childItem);
                }
            }
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
                    IsRosterTitleChanged(diff as InterviewTreeRosterDiff) ||
                    IsAnswerByQuestionChanged(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionValid(diff as InterviewTreeQuestionDiff) ||
                    IsQuestionInalid(diff as InterviewTreeQuestionDiff) ||
                    IsStaticTextValid(diff as InterviewTreeStaticTextDiff) ||
                    IsStaticTextInalid(diff as InterviewTreeStaticTextDiff) ||
                    IsVariableChanged(diff as InterviewTreeVariableDiff) ||
                    IsOptionsSetChanged(diff as InterviewTreeQuestionDiff))
                .ToReadOnlyCollection();
        }

        private bool IsOptionsSetChanged(InterviewTreeQuestionDiff diffByQuestion)
        {
            if (diffByQuestion == null || diffByQuestion.IsNodeRemoved) return false;

            return diffByQuestion.AreLinkedOptionsChanged;
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
            return CreateQuestion(this.questionnaire, questionIdentity);
        }

        public static InterviewTreeQuestion CreateQuestion(IQuestionnaire questionnaire, Identity questionIdentity)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionIdentity.Id);
            string title = questionnaire.GetQuestionTitle(questionIdentity.Id);
            string variableName = questionnaire.GetQuestionVariableName(questionIdentity.Id);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionIdentity.Id);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionIdentity.Id);

            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionIdentity.Id);
            Guid? sourceForLinkedQuestion = null;
            Identity commonParentRosterForLinkedQuestion = null;

            var isLinkedToQuestion = questionnaire.IsQuestionLinked(questionIdentity.Id);
            var isLinkedToRoster = questionnaire.IsQuestionLinkedToRoster(questionIdentity.Id);

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

            return new InterviewTreeQuestion(questionIdentity, questionType: questionType, isDisabled: false,
                title: title, variableName: variableName, answer: null, linkedOptions: null,
                cascadingParentQuestionId: cascadingParentQuestionId, isYesNo: isYesNoQuestion,
                isDecimal: isDecimalQuestion, linkedSourceId: sourceForLinkedQuestion,
                commonParentRosterIdForLinkedQuestion: commonParentRosterForLinkedQuestion);
        }

        public static InterviewTreeVariable CreateVariable(Identity variableIdentity)
        {
            return new InterviewTreeVariable(variableIdentity);
        }

        public InterviewTreeSubSection CreateSubSection(Identity subSectionIdentity)
        {
            return CreateSubSection(this.questionnaire, subSectionIdentity);
        }

        public static InterviewTreeSubSection CreateSubSection(IQuestionnaire questionnaire, Identity subSectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(subSectionIdentity.Id);

            return new InterviewTreeSubSection(subSectionIdentity, childrenReferences);
        }

        public static InterviewTreeSection CreateSection(IQuestionnaire questionnaire, Identity sectionIdentity)
        {
            var childrenReferences = questionnaire.GetChidrenReferences(sectionIdentity.Id);
            return new InterviewTreeSection(sectionIdentity, childrenReferences);
        }

        public static InterviewTreeStaticText CreateStaticText(Identity staticTextIdentity)
        {
            return new InterviewTreeStaticText(staticTextIdentity);
        }

        public RosterManager GetRosterManager(Guid rosterId)
        {
            if (questionnaire.IsFixedRoster(rosterId))
            {
                return new FixedRosterManager(this, this.questionnaire, rosterId);
            }

            Guid sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            var questionaType = questionnaire.GetQuestionType(sourceQuestionId);
            if (questionaType == QuestionType.MultyOption)
            {
                if (this.questionnaire.IsQuestionYesNo(sourceQuestionId))
                {
                    return new YesNoRosterManager(this, this.questionnaire, rosterId);
                }
                return new MultiRosterManager(this, this.questionnaire, rosterId);
            }

            if (questionaType == QuestionType.Numeric)
            {
                return new NumericRosterManager(this, this.questionnaire, rosterId);
            }

            if (questionaType == QuestionType.TextList)
            {
                return new ListRosterManager(this, this.questionnaire, rosterId);
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
            var nodesToRemove =
                 this.nodesCache[identity].TreeToEnumerable<IInterviewTreeNode>(node => node.Children)
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
    }

    public interface IInterviewTreeNode
    {
        Identity Identity { get; }
        IInterviewTreeNode Parent { get; }
        IReadOnlyCollection<IInterviewTreeNode> Children { get; }

        bool IsDisabled();

        void Disable();
        void Enable();

        IInterviewTreeNode Clone();
    }

    public interface IInternalInterviewTreeNode
    {
        void SetTree(InterviewTree tree);
        void SetParent(IInterviewTreeNode parent);
    }

    public abstract class InterviewTreeLeafNode : IInterviewTreeNode, IInternalInterviewTreeNode
    {
        private bool isDisabled;

        protected InterviewTreeLeafNode(Identity identity, bool isDisabled)
        {
            this.Identity = identity;
            this.isDisabled = isDisabled;
        }

        public Identity Identity { get; }
        public InterviewTree Tree { get; private set; }
        public IInterviewTreeNode Parent { get; private set; }
        IReadOnlyCollection<IInterviewTreeNode> IInterviewTreeNode.Children { get; } = Enumerable.Empty<IInterviewTreeNode>().ToReadOnlyCollection();

        void IInternalInterviewTreeNode.SetTree(InterviewTree tree) => this.Tree = tree;
        void IInternalInterviewTreeNode.SetParent(IInterviewTreeNode parent) => this.Parent = parent;

        public bool IsDisabled() => this.isDisabled || (this.Parent?.IsDisabled() ?? false);

        public void Disable() => this.isDisabled = true;
        public void Enable() => this.isDisabled = false;

        public abstract IInterviewTreeNode Clone();
    }

    public enum QuestionnaireReferenceType
    {
        SubSection = 1,
        Roster = 2,
        StaticText = 10,
        Variable = 20,
        Question = 30,
    }

    public class QuestionnaireItemReference
    {
        public QuestionnaireItemReference(QuestionnaireReferenceType type, Guid id)
        {
            this.Type = type;
            this.Id = id;
        }

        public Guid Id { get; set; }

        public QuestionnaireReferenceType Type { get; set; }
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