using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeStaticText : InterviewTreeLeafNode, ISubstitutable, IInterviewTreeValidateable
    {
        public sealed override SubstitutionText Title { get; protected set; }

        public SubstitutionText[] ValidationMessages { get; private set; }

        public InterviewTreeStaticText(Identity identity, SubstitutionText title, SubstitutionText[] validationMessages = null)
            : base(identity)
        {
            this.Title = title;
            this.ValidationMessages = validationMessages ?? new SubstitutionText[0];
            this.FailedErrors = new List<FailedValidationCondition>();
            this.FailedWarnings = new List<FailedValidationCondition>();
        }

        public bool IsValid => this.FailedErrors.Count == 0;
        public bool IsPlausible => this.FailedWarnings.Count == 0;

        public IReadOnlyList<FailedValidationCondition> FailedErrors { get; private set; }
        public IReadOnlyList<FailedValidationCondition> FailedWarnings { get; private set; }

        public void SetTitle(SubstitutionText title)
        {
            this.Title = title;
            this.Title.SetTree(this.Tree);
        }

        public void SetValidationMessages(SubstitutionText[] validationMessages)
        {
            if (validationMessages == null) throw new ArgumentNullException(nameof(validationMessages));
            this.ValidationMessages = validationMessages;
            foreach (var validationMessage in validationMessages)
            {
                validationMessage.SetTree(this.Tree);
            }
        }

        public void MarkInvalid(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if (failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedErrors = failedValidations.ToReadOnlyCollection();
        }
        public void MarkValid()
            => this.FailedErrors = new List<FailedValidationCondition>();

        public void MarkImplausible(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if (failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedWarnings = failedValidations.ToReadOnlyCollection();
        }

        public void MarkPlausible()
            => this.FailedWarnings = new List<FailedValidationCondition>();

        public override string ToString()
            => $"StaticText {Identity} '{Title}'. " +
               $" {(this.IsDisabled() ? "Disabled" : "Enabled")}. " +
               $"{(this.IsValid ? "Valid" : "Invalid")}";

        public override IInterviewTreeNode Clone()
        {
            var clone = (InterviewTreeStaticText)this.MemberwiseClone();
            clone.Title = this.Title?.Clone();
            clone.ValidationMessages = this.ValidationMessages.Select(x => x.Clone()).ToArray();
            clone.FailedErrors = this.FailedErrors
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            clone.FailedWarnings = this.FailedWarnings
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            return clone;
        }

        public override void Accept(IInterviewTreeUpdater updater)
        {
            updater.UpdateEnablement(this);
        }

        public void AcceptValidity(IInterviewTreeUpdater updater)
        {
            updater.UpdateValidations(this); 
        }

        public void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions();
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.ReplaceSubstitutions();
            }
        }
        public override void SetTree(InterviewTree tree)
        {
            base.SetTree(tree);
            this.Title?.SetTree(tree);
            foreach (var messagesWithSubstition in this.ValidationMessages)
            {
                messagesWithSubstition.SetTree(tree);
            }
        }
    }
}
