using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class VariableValueChanges
    {
        public VariableValueChanges():this(null)
        {
        }

        public VariableValueChanges(Dictionary<Identity, object> changedVariableValues)
        {
            this.ChangedVariableValues = changedVariableValues?? new Dictionary<Identity, object>();
        }

        public Dictionary<Identity, object> ChangedVariableValues { get; }


        public void AppendChanges(VariableValueChanges changes)
        {
            foreach (var variableValueChange in changes.ChangedVariableValues)
            {
                this.ChangedVariableValues.Add(variableValueChange.Key,variableValueChange.Value);
            }
        }
    }
}