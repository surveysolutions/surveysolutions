using System.Diagnostics;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{  
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    [DebuggerDisplay("VariableValue = {VariableValue}, VariableIdentity = {VariableIdentity}")]
    public class ChangedVariableValueDto
    {
        public Identity VariableIdentity { get; private set; }
        public object VariableValue { private set; get; }

        public ChangedVariableValueDto(Identity variableIdentity, object variableValue)
        {
            this.VariableIdentity = variableIdentity;
            this.VariableValue = variableValue;
        }
    }
}