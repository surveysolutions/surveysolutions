using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeValidateableDiff : InterviewTreeNodeDiff
    {
        public InterviewTreeValidateableDiff(IInterviewTreeNode sourceNode, IInterviewTreeNode changedNode)
            : base(sourceNode, changedNode)
        {
            var source = sourceNode as IInterviewTreeValidateable;
            var result = changedNode as IInterviewTreeValidateable;

            ChangedNodeBecameValid = source == null || !source.IsValid && result.IsValid;
            ChangedNodeBecameInvalid = ChangedNodeBecameInvalidImp(source, result) ;
            AreValidationMessagesChanged = AreValidationMessagesChangedIml(source, result);
            IsFailedValidationIndexChanged = IsFailedValidationIndexChangedIml(source, result);
        }

        private bool ChangedNodeBecameInvalidImp(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            return source == null ? !result.IsValid : source.IsValid && !result.IsValid;
        }

        public bool AreValidationMessagesChangedIml(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            if (result.IsValid) return false;
            var changedMessages = result.ValidationMessages.Select(x => x.Text).ToArray();
            if (this.IsNodeAdded && !changedMessages.Any()) return false;
            var validationMessages = source?.ValidationMessages;
            var sourceMessages = validationMessages?.Select(x => x.Text) ?? new string[0];
            return !changedMessages.SequenceEqual(sourceMessages);
        }

        public bool IsFailedValidationIndexChangedIml(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            if (result.IsValid) return false;
            var targetChangedValidations = result.FailedValidations ?? new List<FailedValidationCondition>();
            if (this.IsNodeAdded && !targetChangedValidations.Any()) return false;

            var sourceMessages = source?.FailedValidations ?? new List<FailedValidationCondition>();
            return !targetChangedValidations.SequenceEqual(sourceMessages);
        }

        public bool ChangedNodeBecameValid { get; private set; }

        public bool ChangedNodeBecameInvalid { get; private set; }

        public bool AreValidationMessagesChanged { get; private set; }

        public bool IsFailedValidationIndexChanged { get; private set; }
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