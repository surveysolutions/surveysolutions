using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeQuestionDiff : InterviewTreeValidateableDiff
    {
        public new InterviewTreeQuestion SourceNode => base.SourceNode as InterviewTreeQuestion;
        public new InterviewTreeQuestion ChangedNode => base.ChangedNode as InterviewTreeQuestion;

        public InterviewTreeQuestionDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }

        public bool IsTitleChanged
        {
            get
            {
                if (this.IsNodeRemoved) return false;
                if (this.IsNodeAdded && !this.ChangedNode.Title.HasSubstitutions) return false;
                return this.SourceNode?.Title.Text != this.ChangedNode.Title.Text;
            }
        }

        public bool IsAnswerRemoved => this.SourceNode != null && this.SourceNode.IsAnswered() &&
            (this.ChangedNode == null || !this.ChangedNode.IsAnswered());

        public bool IsAnswerChanged
        {
            get
            {
                if (this.ChangedNode == null) return false;

                if (this.SourceNode == null) return this.ChangedNode.IsAnswered();

                if ((this.SourceNode.IsAnswered() && !this.ChangedNode.IsAnswered()) ||
                    (!this.SourceNode.IsAnswered() && this.ChangedNode.IsAnswered())) return true;

                return !this.SourceNode.InterviewQuestion.EqualByAnswer(this.ChangedNode.InterviewQuestion);
                
            }
        }
        public bool AreLinkedOptionsChanged
        {
            get
            {
                if (this.ChangedNode == null) return false;
                if (!this.ChangedNode.IsLinked) return false;

                var sourceOptions = this.SourceNode?.AsLinked.Options ?? new List<RosterVector>();

                if (sourceOptions.Count != this.ChangedNode.AsLinked.Options.Count)
                    return true;

                return !sourceOptions.SequenceEqual(this.ChangedNode.AsLinked.Options);
            }
        }

        public bool AreLinkedToListOptionsChanged
        {
            get
            {
                if (this.ChangedNode == null) return false;
                if (!this.ChangedNode.IsLinkedToListQuestion) return false;

                var sourceOptions = this.SourceNode?.AsLinkedToList.Options ?? EmptyArray<int>.Value;

                if (sourceOptions.Length != this.ChangedNode.AsLinkedToList.Options.Length)
                    return true;

                return !sourceOptions.SequenceEqual(this.ChangedNode.AsLinkedToList.Options);
            }
        }

        public bool NodeIsMarkedAsReadonly
        {
            get
            {
                if (this.ChangedNode == null) return false;
                if (this.IsNodeAdded) return this.ChangedNode.IsReadonly;
                return !SourceNode.IsReadonly && this.ChangedNode.IsReadonly;
            }
        }
    }
}