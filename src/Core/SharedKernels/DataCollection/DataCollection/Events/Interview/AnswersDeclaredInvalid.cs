using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AnswersDeclaredInvalid : InterviewPassiveEvent
    {
        public Identity[] Questions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions { get; protected set; }

        protected AnswersDeclaredInvalid()
        {
            this.Questions = new Identity[] {};
            this.FailedValidationConditions = new FailedValidationConditionsDictionary();
        }

        public AnswersDeclaredInvalid(Identity[] questions)
        {
            this.Questions = questions;

            var dictionary  = new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>();
            foreach (var question in questions)
            {
                dictionary.Add(question, new List<FailedValidationCondition>());
            }

            this.FailedValidationConditions = new FailedValidationConditionsDictionary(dictionary);
        }

        public AnswersDeclaredInvalid(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions)
        {
            this.Questions = failedValidationConditions.Keys.ToArray();
            this.FailedValidationConditions = new FailedValidationConditionsDictionary(failedValidationConditions);
        }
    }
}