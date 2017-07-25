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
        public SubstitutionText Title { get; private set; }

        public SubstitutionText[] ValidationMessages { get; private set; }

        public InterviewTreeStaticText(Identity identity, SubstitutionText title, SubstitutionText[] validationMessages = null)
            : base(identity)
        {
            this.Title = title;
            this.ValidationMessages = validationMessages ?? new SubstitutionText[0];
            this.FailedValidations = new List<FailedValidationCondition>();
        }

        public bool IsValid => this.FailedValidations.Count == 0;

        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; private set; }

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
            this.FailedValidations = failedValidations.ToReadOnlyCollection();
        }
        public void MarkValid()
            => this.FailedValidations = new List<FailedValidationCondition>();

        public override string ToString()
            => $"StaticText {Identity} '{Title}'. " +
               $" {(this.IsDisabled() ? "Disabled" : "Enabled")}. " +
               $"{(this.IsValid ? "Valid" : "Invalid")}";

        public override IInterviewTreeNode Clone()
        {
            var clone = (InterviewTreeStaticText)this.MemberwiseClone();
            clone.Title = this.Title?.Clone();
            clone.ValidationMessages = this.ValidationMessages.Select(x => x.Clone()).ToArray();
            clone.FailedValidations = this.FailedValidations
                .Select(v => new FailedValidationCondition(v.FailedConditionIndex))
                .ToReadOnlyCollection();
            return clone;
        }

        public override void Accept(IInterviewTreeUpdater updater)
        {
            updater.UpdateEnablement(this);
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