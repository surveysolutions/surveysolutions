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

            ChangedNodeBecameValid = ChangedNodeBecameValidImp(source, result);
            ChangedNodeBecameInvalid = ChangedNodeBecameInvalidImp(source, result) ;
            AreValidationMessagesChanged = AreValidationMessagesChangedIml(source, result);
            IsFailedErrorValidationIndexChanged = IsFailedErrorValidationIndexChangedIml(source, result);

            ChangedNodeBecamePlausibled = ChangedNodeBecamePlausibledImp(source, result);
            ChangedNodeBecameImplausibled = ChangedNodeBecameImplausibledImp(source, result);
            IsFailedWarningValidationIndexChanged = IsFailedWarningValidationIndexChangedIml(source, result);
        }

        private bool ChangedNodeBecamePlausibledImp(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            return source == null || source.FailedWarnings.Count != 0 && result.FailedWarnings.Count == 0;
        }

        private bool ChangedNodeBecameImplausibledImp(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            var targetIsImplausibled = result.FailedWarnings.Count != 0;
            return source == null ? targetIsImplausibled : source.FailedWarnings.Count == 0 && targetIsImplausibled;
        }

        private bool IsFailedWarningValidationIndexChangedIml(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            if (result.FailedWarnings.Count == 0) return false;
            var targetChangedValidations = result.FailedWarnings ?? new List<FailedValidationCondition>();
            if (this.IsNodeAdded && !targetChangedValidations.Any()) return false;

            var sourceMessages = source?.FailedWarnings ?? new List<FailedValidationCondition>();
            return !targetChangedValidations.SequenceEqual(sourceMessages);
        }

        private bool ChangedNodeBecameValidImp(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            return source == null || !source.IsValid && result.IsValid;
        }

        private bool ChangedNodeBecameInvalidImp(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            return source == null ? !result.IsValid : source.IsValid && !result.IsValid;
        }

        private bool AreValidationMessagesChangedIml(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            if (result.IsValid && result.IsPlausible) return false;
            var changedMessages = result.ValidationMessages.Select(x => x.Text).ToArray();
            if (this.IsNodeAdded && !changedMessages.Any()) return false;
            var validationMessages = source?.ValidationMessages;
            var sourceMessages = validationMessages?.Select(x => x.Text) ?? new string[0];
            return !changedMessages.SequenceEqual(sourceMessages);
        }

        private bool IsFailedErrorValidationIndexChangedIml(IInterviewTreeValidateable source, IInterviewTreeValidateable result)
        {
            if (this.IsNodeRemoved) return false;
            if (result.IsValid) return false;
            var targetChangedValidations = result.FailedErrors ?? new List<FailedValidationCondition>();
            if (this.IsNodeAdded && !targetChangedValidations.Any()) return false;

            var sourceMessages = source?.FailedErrors ?? new List<FailedValidationCondition>();
            return !targetChangedValidations.SequenceEqual(sourceMessages);
        }

        public bool AreValidationMessagesChanged { get; private set; }

        public bool ChangedNodeBecameValid { get; private set; }
        public bool ChangedNodeBecameInvalid { get; private set; }
        public bool IsFailedErrorValidationIndexChanged { get; private set; }

        public bool ChangedNodeBecamePlausibled { get; private set; }
        public bool ChangedNodeBecameImplausibled { get; private set; }
        public bool IsFailedWarningValidationIndexChanged { get; private set; }
    }

    public interface IInterviewTreeValidateable
    {
        bool IsValid { get; }

        bool IsPlausible { get; }

        SubstitutionText[] ValidationMessages { get; }

        IReadOnlyList<FailedValidationCondition> FailedErrors { get; }
        void MarkInvalid(IEnumerable<FailedValidationCondition> failedValidations);
        void MarkValid();

        IReadOnlyList<FailedValidationCondition> FailedWarnings { get; }
        void MarkImplausible(IEnumerable<FailedValidationCondition> failedValidations);
        void MarkPlausible();

        void AcceptValidity(IInterviewTreeUpdater updater);
    }
}
