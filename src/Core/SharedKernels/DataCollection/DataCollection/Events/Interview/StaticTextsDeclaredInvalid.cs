using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredInvalid : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (this.failedValidationConditionsDictionary
                = this.FailedValidationConditions.ToDictionary());

        protected StaticTextsDeclaredInvalid()
        {
            this.FailedValidationConditions = new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>();;
        }

        public StaticTextsDeclaredInvalid(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}