using System.Diagnostics;

namespace WB.Services.Export.Events.Interview.Dtos
{  
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    [DebuggerDisplay("NewValue = {NewValue}, Identity = {Identity}")]
    public class ChangedVariable
    {
        public Identity Identity { get; private set; }
        public object NewValue { private set; get; }

        public ChangedVariable(Identity identity, object newValue)
        {
            this.Identity = identity;
            this.NewValue = newValue;
        }
    }
}