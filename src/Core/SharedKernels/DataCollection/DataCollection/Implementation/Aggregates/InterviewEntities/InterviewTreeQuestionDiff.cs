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

                if (this.SourceNode.IsText) return !this.SourceNode.AsText.EqualByAnswer(this.ChangedNode.AsText);
                if (this.SourceNode.IsInteger) return !this.SourceNode.AsInteger.EqualByAnswer(this.ChangedNode.AsInteger);
                if (this.SourceNode.IsDouble) return !this.SourceNode.AsDouble.EqualByAnswer(this.ChangedNode.AsDouble);
                if (this.SourceNode.IsDateTime) return !this.SourceNode.AsDateTime.EqualByAnswer(this.ChangedNode.AsDateTime);
                if (this.SourceNode.IsMultimedia) return !this.SourceNode.AsMultimedia.EqualByAnswer(this.ChangedNode.AsMultimedia);
                if (this.SourceNode.IsQRBarcode) return !this.SourceNode.AsQRBarcode.EqualByAnswer(this.ChangedNode.AsQRBarcode);
                if (this.SourceNode.IsGps) return !this.SourceNode.AsGps.EqualByAnswer(this.ChangedNode.AsGps);
                if (this.SourceNode.IsSingleFixedOption) return !this.SourceNode.AsSingleFixedOption.EqualByAnswer(this.ChangedNode.AsSingleFixedOption);
                if (this.SourceNode.IsSingleLinkedOption) return !this.SourceNode.AsSingleLinkedOption.EqualByAnswer(this.ChangedNode.AsSingleLinkedOption);
                if (this.SourceNode.IsMultiFixedOption) return !this.SourceNode.AsMultiFixedOption.EqualByAnswer(this.ChangedNode.AsMultiFixedOption);
                if (this.SourceNode.IsMultiLinkedOption) return !this.SourceNode.AsMultiLinkedOption.EqualByAnswer(this.ChangedNode.AsMultiLinkedOption);
                if (this.SourceNode.IsYesNo) return !this.SourceNode.AsYesNo.EqualByAnswer(this.ChangedNode.AsYesNo);
                if (this.SourceNode.IsTextList) return !this.SourceNode.AsTextList.EqualByAnswer(this.ChangedNode.AsTextList);
                if (this.SourceNode.IsSingleLinkedToList) return !this.SourceNode.AsSingleLinkedToList.EqualByAnswer(this.ChangedNode.AsSingleLinkedToList);
                if (this.SourceNode.IsMultiLinkedToList) return !this.SourceNode.AsMultiLinkedToList.EqualByAnswer(this.ChangedNode.AsMultiLinkedToList);
                if (this.SourceNode.IsArea) return !this.SourceNode.AsArea.EqualByAnswer(this.ChangedNode.AsArea);
                if (this.SourceNode.IsAudio) return !this.SourceNode.AsAudio.EqualByAnswer(this.ChangedNode.AsAudio);

                return false;
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