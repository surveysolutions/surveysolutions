using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;


namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredImplausible : InterviewPassiveEvent
    {
        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions { get; protected set; }

        protected StaticTextsDeclaredImplausible()
        {
            this.FailedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>(); 
        }

        public StaticTextsDeclaredImplausible(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}