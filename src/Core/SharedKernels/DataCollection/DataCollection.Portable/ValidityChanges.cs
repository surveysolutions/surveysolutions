using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class ValidityChanges
    {
        internal ValidityChanges() : this(null, null) { }
        public ValidityChanges(List<Identity> answersDeclaredValid, List<Identity> answersDeclaredInvalid)
        {
            this.AnswersDeclaredValid = answersDeclaredValid ?? new List<Identity>();
            this.AnswersDeclaredInvalid = answersDeclaredInvalid ?? new List<Identity>();
            this.FailedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
        }

        public ValidityChanges(List<Identity> answersDeclaredValid, List<Identity> answersDeclaredInvalid, IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
            :this(answersDeclaredValid,answersDeclaredInvalid)
        {
            this.FailedValidationConditions = failedValidationConditions ?? new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
        }

        public void AppendChanges(ValidityChanges changes)
        {
            this.AnswersDeclaredValid.AddRange(changes.AnswersDeclaredValid);
            this.AnswersDeclaredInvalid.AddRange(changes.AnswersDeclaredInvalid);
            this.FailedValidationConditions = this.FailedValidationConditions.Union(changes.FailedValidationConditions).ToDictionary(k => k.Key, v => v.Value);
        }

        public List<Identity> AnswersDeclaredValid { get; private set; }

        public List<Identity> AnswersDeclaredInvalid { get; private set; }

        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions { get; private set; }

        public void Clear()
        {
            this.AnswersDeclaredInvalid.Clear();
            this.AnswersDeclaredValid.Clear();
            this.FailedValidationConditions.Clear();
        }
    }
}