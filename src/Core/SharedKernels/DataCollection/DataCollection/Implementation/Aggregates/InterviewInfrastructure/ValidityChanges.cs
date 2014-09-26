using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class ValidityChanges
    {
        public ValidityChanges(List<Identity> answersDeclaredValid, List<Identity> answersDeclaredInvalid)
        {
            this.AnswersDeclaredValid = answersDeclaredValid ?? new List<Identity>();
            this.AnswersDeclaredInvalid = answersDeclaredInvalid ?? new List<Identity>();
        }

        public List<Identity> AnswersDeclaredValid { get; private set; }
        public List<Identity> AnswersDeclaredInvalid { get; private set; }

        public void Clear()
        {
            this.AnswersDeclaredInvalid.Clear();
            this.AnswersDeclaredValid.Clear();
        }
    }
}