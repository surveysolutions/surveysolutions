using System.Globalization;
using System.Linq;

namespace WB.Core.GenericSubdomains.Portable
{
    // TODO: this should be replaced with usage of RosterVector type.
    public static class EventHandlerUtils
    {
        public static string CreateLeveKeyFromPropagationVector(this decimal[] vector)
        {
            return string.Join(",", vector.Select(v => v.ToString("0.############################", CultureInfo.InvariantCulture)));
        }
    }
}