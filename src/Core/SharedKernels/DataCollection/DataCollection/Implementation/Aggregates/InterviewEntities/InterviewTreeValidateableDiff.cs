using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeValidateableDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeValidateableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
        }

        private IInterviewTreeValidateable source => this.SourceNode as IInterviewTreeValidateable;
        private IInterviewTreeValidateable result => this.ChangedNode as IInterviewTreeValidateable;

        public bool ChangedNodeBecameValid => this.source == null || !this.source.IsValid && this.result.IsValid;

        public bool ChangedNodeBecameInvalid => this.source == null
            ? !this.result.IsValid
            : this.source.IsValid && !this.result.IsValid;

        public bool AreValidationMessagesChanged
        {
            get
            {
                if (this.IsNodeRemoved) return false;
                if (this.result.IsValid) return false;
                var changedMessages = this.result.ValidationMessages.Select(x => x.Text).ToArray();
                if (this.IsNodeAdded && !changedMessages.Any()) return false;
                var validationMessages = this.source?.ValidationMessages;
                var sourceMessages = validationMessages?.Select(x => x.Text) ?? new string[0];
                return !changedMessages.SequenceEqual(sourceMessages);
            }
        }

        public bool IsFailedValidationIndexChanged
        {
            get
            {
                if (this.IsNodeRemoved) return false;
                if (this.result.IsValid) return false;
                var targetChangedValidations = this.result.FailedValidations;
                if (this.IsNodeAdded && !targetChangedValidations.Any()) return false;

                var sourceMessages = this.source?.FailedValidations ?? new List<FailedValidationCondition>();
                return !targetChangedValidations.SequenceEqual(sourceMessages);
            }
        }
    }

    public interface IInterviewTreeValidateable
    {
        bool IsValid { get; }
        SubstitutionText[] ValidationMessages { get; }
        IReadOnlyList<FailedValidationCondition> FailedValidations { get; }

        void MarkInvalid(IEnumerable<FailedValidationCondition> failedValidations);

        void MarkValid();
    }
}