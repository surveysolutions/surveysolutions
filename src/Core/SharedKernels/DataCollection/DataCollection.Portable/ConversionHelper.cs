using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class ConversionHelper
    {
        /// <remarks>
        /// The opposite operation (get id or vector from string) should never be performed!
        /// This is one-way transformation. Opposite operation is too slow.
        /// If you need to compactify data and get it back, you should use another datatype, not a string.
        /// </remarks>
        public static string ConvertIdAndRosterVectorToString(Guid id, decimal[] rosterVector = null)
        {
            if (rosterVector == null || !rosterVector.Any())
                return id.ToString("N");

            return String.Format("{0}[{1}]", id.ToString("N"),
                String.Join((string)"-",
                    (IEnumerable<string>)
                        rosterVector.Select(
                            v => v.ToString("0.############################", CultureInfo.InvariantCulture))));
        }

        public static string ConvertIdentityToString(Identity identity)
        {
            return ConvertIdAndRosterVectorToString(identity.Id, identity.RosterVector);
        }
    }
}
