using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeStaticText : InterviewTreeLeafNode
    {
        public SubstitionText Title { get; private set; }

        public InterviewTreeStaticText(Identity identity, SubstitionText title)
            : base(identity)
        {
            this.Title = title;
        }

        public bool IsValid => !this.FailedValidations?.Any() ?? true;
        public IReadOnlyList<FailedValidationCondition> FailedValidations { get; private set; }
        public void MarkAsInvalid(IEnumerable<FailedValidationCondition> failedValidations)
        {
            if (failedValidations == null) throw new ArgumentNullException(nameof(failedValidations));
            this.FailedValidations = failedValidations.ToReadOnlyCollection();
        }
        public void MarkAsValid()
            => this.FailedValidations = Enumerable.Empty<FailedValidationCondition>().ToList();

        public override string ToString() => $"Text ({this.Identity})";
        public override IInterviewTreeNode Clone()
        {
            var clone = (InterviewTreeStaticText)this.MemberwiseClone();
            clone.Title = this.Title?.Clone(null);
            return clone;
        }

        public override void ReplaceSubstitutions()
        {
            this.Title.ReplaceSubstitutions();
        }
        public override void SetTree(InterviewTree tree)
        {
            base.SetTree(tree);
            this.Title?.SetTree(tree);
        }
    }
}