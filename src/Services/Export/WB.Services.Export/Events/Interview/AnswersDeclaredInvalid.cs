using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WB.Services.Export.Events.Interview.Base;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Events.Interview
{
    public class AnswersDeclaredInvalid : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>>? failedValidationConditions;

        public Identity[] Questions { get; set; }

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedConditionsStorage { get; protected set; } = null!;

        [JsonIgnore]
        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions
        {
            get
            {
                return this.failedValidationConditions ??= 
                    this.FailedConditionsStorage.ToDictionary(x => x.Key, x => x.Value);
            }
            protected set
            {
                this.FailedConditionsStorage = value?.ToList() ?? new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>();
                this.failedValidationConditions = null;
            }
        }

        public AnswersDeclaredInvalid(
            IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions,
            DateTimeOffset originDate)
        {
            OriginDate = originDate;

            if (failedValidationConditions != null)
            {
                this.Questions = failedValidationConditions.Keys.ToArray();
                this.FailedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>(failedValidationConditions);
            }
            else
            {
                this.Questions = new Identity[] { };
                this.FailedValidationConditions = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
            }
        }
    }
}
