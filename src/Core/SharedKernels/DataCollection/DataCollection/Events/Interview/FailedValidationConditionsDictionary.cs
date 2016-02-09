using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [JsonArray]
    public class FailedValidationConditionsDictionary : ReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>>
    {
        public FailedValidationConditionsDictionary() : base(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>())
        {
        }

        public FailedValidationConditionsDictionary(IDictionary<Identity, IReadOnlyList<FailedValidationCondition>> dictionary) : base(dictionary)
        {
        }
    }
}