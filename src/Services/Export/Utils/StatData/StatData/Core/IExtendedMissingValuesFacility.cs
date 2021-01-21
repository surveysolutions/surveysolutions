using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatData.Core
{
    public interface IExtendedMissingValuesFacility
    {
        /// <summary>
        /// Specifies a value that should be treated as an extended missing 
        /// value when exporting to a format that supports this functionality.
        /// Label for this value may be specified, or the word "missing" will 
        /// be used by default.
        /// 
        /// FIRST IMPLEMENTATION WILL IGNORE THE LABEL!
        /// </summary>
        /// <param name="value">Numeric value to be treated as an extended missing value.</param>
        /// <param name="label">Optional string label for this value.</param>
        void Add(string value, string label);

        bool IsMissing(string value);
        int IndexOf(string value);

        List<ExtendedMissingValue> GetList();
    }
}
