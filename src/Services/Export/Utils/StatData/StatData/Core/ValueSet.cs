using System.Collections.Generic;

namespace StatData.Core
{
    /// <summary>
    /// Represents an value set for a numeric variable
    /// </summary>
    public class ValueSet : Dictionary<double, string>
    {
        // empty value labels will confuse graphing functions!
    }
}
