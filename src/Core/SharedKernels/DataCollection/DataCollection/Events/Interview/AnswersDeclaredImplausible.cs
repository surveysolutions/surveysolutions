using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredImplausible : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (this.failedValidationConditionsDictionary = this.FailedValidationConditions.ToDictionary());

        protected AnswersDeclaredImplausible()
        {
            this.FailedValidationConditions = new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>();
        }

        public AnswersDeclaredImplausible(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}