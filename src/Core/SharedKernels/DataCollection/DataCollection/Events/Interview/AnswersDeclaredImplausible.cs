using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
//using System.Collections.ObjectModel;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredImplausible : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (
                   this.failedValidationConditionsDictionary =
                       this.FailedValidationConditions != null ?
                       (IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>>) this.FailedValidationConditions.ToDictionary() :
                        new ReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>>(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>()));

        public AnswersDeclaredImplausible(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions, 
            DateTimeOffset originDate) : base (originDate)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}
