using System.Collections.Generic;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Infrastructure;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview
{
    public class StaticTextsDeclaredInvalid : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (this.failedValidationConditionsDictionary
                   = this.FailedValidationConditions != null ?
                       this.FailedValidationConditions.ToDictionary() :
                       new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>().ToDictionary());

        public StaticTextsDeclaredInvalid(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}
