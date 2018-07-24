using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeQuestionDiff : InterviewTreeValidateableDiff
    {
        public InterviewTreeQuestionDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) 
            : base(sourceNode, changedNode)
        {
            var sourceQuestionNode = sourceNode as InterviewTreeQuestion;
            var changedQuestionNode = changedNode as InterviewTreeQuestion;
            IsTitleChanged = IsTitleChangedImpl(sourceQuestionNode, changedQuestionNode);
            WereInstructionsChanged = WereInstructionsChangedImpl(sourceQuestionNode, changedQuestionNode);

            IsAnswerRemoved = IsAnswerRemovedImpl(sourceQuestionNode, changedQuestionNode);
            IsAnswerChanged = IsAnswerChangedImpl(sourceQuestionNode, changedQuestionNode);
            AreLinkedOptionsChanged = AreLinkedOptionsChangedImpl(sourceQuestionNode, changedQuestionNode);
            AreLinkedToListOptionsChanged = AreLinkedToListOptionsChangedImpl(sourceQuestionNode, changedQuestionNode);

            NodeIsMarkedAsReadonly = NodeIsMarkedAsReadonlyImpl(sourceQuestionNode, changedQuestionNode);
            AnswersMarkedAsProtected = AnswersMarkedAsProtectedImpl(sourceQuestionNode, changedQuestionNode);
        }

        public bool AnswersMarkedAsProtected { get; }

        public new InterviewTreeQuestion SourceNode => base.SourceNode as InterviewTreeQuestion;
        public new InterviewTreeQuestion ChangedNode => base.ChangedNode as InterviewTreeQuestion;


        public bool IsTitleChanged { get; }

        public bool WereInstructionsChanged { get; }

        public bool IsAnswerRemoved { get; }

        public bool IsAnswerChanged { get; }
        public bool AreLinkedOptionsChanged { get; }
        public bool AreLinkedToListOptionsChanged { get; }

        public bool NodeIsMarkedAsReadonly { get; }

        public bool IsTitleChangedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (IsNodeRemoved) return false;
            if (IsNodeAdded && !changedNode.Title.HasSubstitutions) return false;
            return sourceNode?.Title.Text != changedNode.Title.Text;
        }

        public bool WereInstructionsChangedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (IsNodeRemoved) return false;
            if (IsNodeAdded && !changedNode.Instructions.HasSubstitutions) return false;
            return sourceNode?.Instructions.Text != changedNode.Instructions.Text;
        }

        public bool AnswersMarkedAsProtectedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (changedNode == null) return false;
            if (IsNodeAdded) return changedNode.HasProtectedAnswer();

            return sourceNode.HasProtectedAnswer() != changedNode.HasProtectedAnswer();
        }

        public bool IsAnswerRemovedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            return sourceNode != null && sourceNode.IsAnswered() &&
                    (changedNode == null || !changedNode.IsAnswered());
        }

        public bool IsAnswerChangedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (changedNode == null) return false;

            if (sourceNode == null) return changedNode.IsAnswered();

            if ((sourceNode.IsAnswered() && !changedNode.IsAnswered()) ||
                (!sourceNode.IsAnswered() && changedNode.IsAnswered())) return true;

            return !sourceNode.InterviewQuestion.EqualByAnswer(changedNode.InterviewQuestion);
                
        }
        public bool AreLinkedOptionsChangedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (changedNode == null) return false;
            if (!changedNode.IsLinked) return false;

            var sourceOptions = sourceNode?.AsLinked.Options ?? new List<RosterVector>();

            if (sourceOptions.Count != changedNode.AsLinked.Options.Count)
                return true;

            return !sourceOptions.SequenceEqual(changedNode.AsLinked.Options);
        }

        public bool AreLinkedToListOptionsChangedImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (changedNode == null) return false;
            if (!changedNode.IsLinkedToListQuestion) return false;

            var sourceOptions = sourceNode?.AsLinkedToList.Options ?? EmptyArray<int>.Value;

            if (sourceOptions.Length != changedNode.AsLinkedToList.Options.Length)
                return true;

            return !sourceOptions.SequenceEqual(changedNode.AsLinkedToList.Options);
        }

        public bool NodeIsMarkedAsReadonlyImpl(InterviewTreeQuestion sourceNode, InterviewTreeQuestion changedNode)
        {
            if (changedNode == null) return false;
            if (IsNodeAdded) return changedNode.IsReadonly;
            return !sourceNode.IsReadonly && changedNode.IsReadonly;
        }
    }
}
