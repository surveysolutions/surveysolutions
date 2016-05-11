using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public class VariableValueChanges
    {
        public VariableValueChanges() : this(null)
        {
        }

        public VariableValueChanges(Dictionary<Identity, object> changedVariableValues)
        {
            this.ChangedVariableValues = changedVariableValues ?? new Dictionary<Identity, object>();
        }

        public Dictionary<Identity, object> ChangedVariableValues { get; }

        public static VariableValueChanges Concat(VariableValueChanges first, VariableValueChanges second)
            => new VariableValueChanges(first.ChangedVariableValues.Concat(second.ChangedVariableValues).ToDictionary(s => s.Key, s => s.Value));

        public static VariableValueChanges Concat(IEnumerable<VariableValueChanges> manyChanges)
            => manyChanges.Aggregate(new VariableValueChanges(), Concat);
    }
}