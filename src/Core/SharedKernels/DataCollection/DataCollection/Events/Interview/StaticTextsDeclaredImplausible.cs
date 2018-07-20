using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;


namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredImplausible : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (this.failedValidationConditionsDictionary = 
                   this.FailedValidationConditions != null 
                       ? this.FailedValidationConditions.ToDictionary() 
                       : new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>().ToDictionary());
        

        public StaticTextsDeclaredImplausible(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions, 
            DateTimeOffset originDate): base (originDate)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}
