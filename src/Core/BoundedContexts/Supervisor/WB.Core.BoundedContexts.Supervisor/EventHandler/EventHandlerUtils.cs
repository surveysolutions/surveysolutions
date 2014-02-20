using System.Globalization;
using System.Linq;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class EventHandlerUtils {
        internal static string CreateLeveKeyFromPropagationVector(decimal[] vector)
        {
            return string.Join(",", vector.Select(v=>v.ToString("0.############################", CultureInfo.InvariantCulture)));
        }
    }
}