using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeNodeDiff
    {
        public InterviewTreeNodeDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
        {
            this.SourceNode = sourceNode;
            this.ChangedNode = changedNode;
        }
        public IInterviewTreeNode SourceNode { get; }
        public IInterviewTreeNode ChangedNode { get; }

        public bool IsNodeAdded => this.SourceNode == null && this.ChangedNode != null;
        public bool IsNodeRemoved => this.SourceNode != null && this.ChangedNode == null;

        public bool IsNodeDisabled
        {
            get
            {
                var newNodeIsDisabled = this.SourceNode == null && this.ChangedNode!=null && this.ChangedNode.IsDisabled();
                var existingNodeWasEnadledAndWasDisabled = this.SourceNode != null && this.ChangedNode != null && !this.SourceNode.IsDisabled() && this.ChangedNode.IsDisabled();
                return newNodeIsDisabled || existingNodeWasEnadledAndWasDisabled;
            }
        }

        public bool IsNodeEnabled
        {
            get
            {
                var nodeIsRemovedAndWasDisabled = this.ChangedNode == null && this.SourceNode != null && this.SourceNode.IsDisabled();
                var existingNodeWasEnabledAndNowIsDisabled = this.SourceNode != null && this.ChangedNode !=null && this.SourceNode.IsDisabled() && !this.ChangedNode.IsDisabled();
                return existingNodeWasEnabledAndNowIsDisabled || nodeIsRemovedAndWasDisabled;
            }
        }

        public static InterviewTreeNodeDiff Create(IInterviewTreeNode source, IInterviewTreeNode changed)
        {
            if (source is InterviewTreeRoster || changed is InterviewTreeRoster)
                return new InterviewTreeRosterDiff(source, changed);
            else if (source is InterviewTreeSection || changed is InterviewTreeSection)
                return new InterviewTreeGroupDiff(source, changed);
            else if (source is InterviewTreeGroup || changed is InterviewTreeGroup)
                return new InterviewTreeGroupDiff(source, changed);
            else if (source is InterviewTreeQuestion || changed is InterviewTreeQuestion)
                return new InterviewTreeQuestionDiff(source, changed);
            else if (source is InterviewTreeStaticText || changed is InterviewTreeStaticText)
                return new InterviewTreeStaticTextDiff(source, changed);
            else if (source is InterviewTreeVariable || changed is InterviewTreeVariable)
                return new InterviewTreeVariableDiff(source, changed);

            return new InterviewTreeNodeDiff(source, changed);
        }

        public Identity Identity => this.ChangedNode?.Identity ?? this.SourceNode?.Identity;
    }

    public class InterviewTreeRosterDiff : InterviewTreeGroupDiff
    {
        public new InterviewTreeRoster SourceNode => base.SourceNode as InterviewTreeRoster;
        public new InterviewTreeRoster ChangedNode => base.ChangedNode as InterviewTreeRoster;

        public InterviewTreeRosterDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }

        public bool IsRosterTitleChanged
            => this.ChangedNode != null && this.SourceNode?.RosterTitle != this.ChangedNode.RosterTitle;
    }

    public class InterviewTreeGroupDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeGroup SourceNode => base.SourceNode as InterviewTreeGroup;
        public new InterviewTreeGroup ChangedNode => base.ChangedNode as InterviewTreeGroup;

        public InterviewTreeGroupDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
        public bool IsTitleChanged
        {
            get
            {
                if (IsNodeRemoved) return false;
                if (this.IsNodeAdded && !this.ChangedNode.Title.HasSubstitutions) return false;
                return this.SourceNode?.Title.Text != this.ChangedNode.Title.Text;
            }
        }
    }

    public class InterviewTreeQuestionDiff : InterviewTreeNodeDiff
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
                if (IsNodeRemoved) return false;
                if (this.IsNodeAdded && !this.ChangedNode.Title.HasSubstitutions) return false;
                return this.SourceNode?.Title.Text != this.ChangedNode.Title.Text;
            }
        }

        public bool AreValidationMessagesChanged
        {
            get
            {
                if (IsNodeRemoved) return false;
                if (this.ChangedNode.IsValid) return false;
                var changedMessages = this.ChangedNode.ValidationMessages.Select(x => x.Text).ToArray();
                if (this.IsNodeAdded && !changedMessages.Any()) return false;
                var sourceMessages = this.SourceNode?.ValidationMessages.Select(x => x.Text).ToArray() ?? new string[0];
                return !changedMessages.SequenceEqual(sourceMessages);
            }
        }

        public bool IsValid => this.SourceNode == null || !this.SourceNode.IsValid && this.ChangedNode.IsValid;

        public bool IsInvalid => this.SourceNode == null
            ? !this.ChangedNode.IsValid
            : this.SourceNode.IsValid && !this.ChangedNode.IsValid;

        public bool IsAnswerRemoved => this.SourceNode != null && this.SourceNode.IsAnswered() &&
               (this.ChangedNode == null || !this.ChangedNode.IsAnswered());

        public bool IsAnswerChanged
        {
            get
            {
                if (ChangedNode == null) return false;

                if (SourceNode == null) return ChangedNode.IsAnswered();

                if ((SourceNode.IsAnswered() && !ChangedNode.IsAnswered()) ||
                    (!SourceNode.IsAnswered() && ChangedNode.IsAnswered())) return true;

                if (SourceNode.IsText) return !SourceNode.AsText.EqualByAnswer(ChangedNode.AsText);
                if (SourceNode.IsInteger) return !SourceNode.AsInteger.EqualByAnswer(ChangedNode.AsInteger);
                if (SourceNode.IsDouble) return !SourceNode.AsDouble.EqualByAnswer(ChangedNode.AsDouble);
                if (SourceNode.IsDateTime) return !SourceNode.AsDateTime.EqualByAnswer(ChangedNode.AsDateTime);
                if (SourceNode.IsMultimedia) return !SourceNode.AsMultimedia.EqualByAnswer(ChangedNode.AsMultimedia);
                if (SourceNode.IsQRBarcode) return !SourceNode.AsQRBarcode.EqualByAnswer(ChangedNode.AsQRBarcode);
                if (SourceNode.IsGps) return !SourceNode.AsGps.EqualByAnswer(ChangedNode.AsGps);
                if (SourceNode.IsSingleFixedOption) return !SourceNode.AsSingleFixedOption.EqualByAnswer(ChangedNode.AsSingleFixedOption);
                if (SourceNode.IsSingleLinkedOption) return !SourceNode.AsSingleLinkedOption.EqualByAnswer(ChangedNode.AsSingleLinkedOption);
                if (SourceNode.IsMultiFixedOption) return !SourceNode.AsMultiFixedOption.EqualByAnswer(ChangedNode.AsMultiFixedOption);
                if (SourceNode.IsMultiLinkedOption) return !SourceNode.AsMultiLinkedOption.EqualByAnswer(ChangedNode.AsMultiLinkedOption);
                if (SourceNode.IsYesNo) return !SourceNode.AsYesNo.EqualByAnswer(ChangedNode.AsYesNo);
                if (SourceNode.IsTextList) return !SourceNode.AsTextList.EqualByAnswer(ChangedNode.AsTextList);
                if (SourceNode.IsSingleLinkedToList) return !SourceNode.AsSingleLinkedToList.EqualByAnswer(ChangedNode.AsSingleLinkedToList);
                if (SourceNode.IsMultiLinkedToList) return !SourceNode.AsMultiLinkedToList.EqualByAnswer(ChangedNode.AsMultiLinkedToList);

                return false;
            }
        }

        public bool AreLinkedOptionsChanged
        {
            get
            {
                if (ChangedNode == null) return false;
                if (!ChangedNode.IsLinked) return false;

                var sourceOptions = this.SourceNode?.AsLinked.Options ?? new List<RosterVector>();

                if (sourceOptions.Count  != ChangedNode.AsLinked.Options.Count)
                    return true;

                return !sourceOptions.SequenceEqual(ChangedNode.AsLinked.Options);
            }
        }
    }

    public class InterviewTreeStaticTextDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeStaticText SourceNode => base.SourceNode as InterviewTreeStaticText;
        public new InterviewTreeStaticText ChangedNode => base.ChangedNode as InterviewTreeStaticText;

        public InterviewTreeStaticTextDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }

        public bool IsValid => this.SourceNode == null || !this.SourceNode.IsValid && this.ChangedNode.IsValid;

        public bool IsInvalid => this.SourceNode == null
            ? !this.ChangedNode.IsValid
            : this.SourceNode.IsValid && !this.ChangedNode.IsValid;

        public bool IsTitleChanged
        {
            get
            {
                if (IsNodeRemoved) return false;
                if (this.IsNodeAdded && !this.ChangedNode.Title.HasSubstitutions) return false;
                return this.SourceNode?.Title.Text != this.ChangedNode.Title.Text;
            }
        }

        public bool AreValidationMessagesChanged
        {
            get
            {
                if (IsNodeRemoved) return false;
                if (this.ChangedNode.IsValid) return false;
                var changedMessages = this.ChangedNode.ValidationMessages.Select(x => x.Text).ToArray();
                if (this.IsNodeAdded && !changedMessages.Any()) return false;
                var sourceMessages = this.SourceNode?.ValidationMessages.Select(x => x.Text).ToArray() ?? new string[0];
                return !changedMessages.SequenceEqual(sourceMessages);
            }
        }
    }

    public class InterviewTreeVariableDiff : InterviewTreeNodeDiff
    {
        public new InterviewTreeVariable SourceNode => base.SourceNode as InterviewTreeVariable;
        public new InterviewTreeVariable ChangedNode => base.ChangedNode as InterviewTreeVariable;

        public bool IsValueChanged => this.SourceNode == null
            ? this.ChangedNode.HasValue
            : this.SourceNode.Value != this.ChangedNode.Value;

        public InterviewTreeVariableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode) : base(sourceNode, changedNode)
        {
        }
    }
}