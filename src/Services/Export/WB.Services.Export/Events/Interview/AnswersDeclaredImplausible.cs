using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WB.Services.Export.Events.Interview.Base;
//using System.Collections.ObjectModel;

namespace WB.Services.Export.Events.Interview
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
    }
}
