using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class ValidityChanges
    {
        internal ValidityChanges() : this(null, null) { }

        public ValidityChanges(
            List<Identity> answersDeclaredValid,
            List<Identity> answersDeclaredInvalid)
        {
            this.AnswersDeclaredValid = answersDeclaredValid ?? new List<Identity>();

            this.AnswersDeclaredInvalid = answersDeclaredInvalid ?? new List<Identity>();

            IReadOnlyList<FailedValidationCondition> singleFailedValidationList = new[] { new FailedValidationCondition(0) };
            this.FailedValidationConditions = this.AnswersDeclaredInvalid.ToDictionary(q => q, q => singleFailedValidationList);

            this.StaticTextsDeclaredValid = new List<Identity>();
            this.FailedValidationConditionsForStaticTexts = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
        }

        public ValidityChanges(
            List<Identity> answersDeclaredValid, 
            List<Identity> answersDeclaredInvalid, 
            IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
            : this(
                answersDeclaredValid,
                answersDeclaredInvalid,
                failedValidationConditions,
                null,
                null) {}

        public ValidityChanges(
            List<Identity> answersDeclaredValid, 
            List<Identity> answersDeclaredInvalid, 
            IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions,
            List<Identity> staticTextsDeclaredValid,
            IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedStaticTextsValidationConditions)
            : this(answersDeclaredValid, answersDeclaredInvalid)
        {
            this.FailedValidationConditions = failedValidationConditions ?? new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();

            this.StaticTextsDeclaredValid = staticTextsDeclaredValid ?? new List<Identity>();
            this.FailedValidationConditionsForStaticTexts = failedStaticTextsValidationConditions ?? new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
        }

        public void AppendChanges(ValidityChanges changes)
        {
            this.AnswersDeclaredValid.AddRange(changes.AnswersDeclaredValid);
            this.AnswersDeclaredInvalid.AddRange(changes.AnswersDeclaredInvalid);
            this.FailedValidationConditions = this.FailedValidationConditions.Union(changes.FailedValidationConditions).ToDictionary(k => k.Key, v => v.Value);

            this.StaticTextsDeclaredValid.AddRange(changes.StaticTextsDeclaredValid);
            this.FailedValidationConditionsForStaticTexts = this.FailedValidationConditionsForStaticTexts.Union(changes.FailedValidationConditionsForStaticTexts).ToDictionary(k => k.Key, v => v.Value);
        }

        public List<Identity> AnswersDeclaredValid { get; }
        public List<Identity> StaticTextsDeclaredValid { get; }

        public List<Identity> AnswersDeclaredInvalid { get; }

        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions { get; private set; }

        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditionsForQuestions => FailedValidationConditions;

        public IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditionsForStaticTexts { get; private set; }

        public void Clear()
        {
            this.AnswersDeclaredInvalid.Clear();
            this.AnswersDeclaredValid.Clear();
            this.FailedValidationConditions.Clear();

            this.StaticTextsDeclaredValid.Clear();
            this.FailedValidationConditionsForStaticTexts.Clear();
        }
    }
}