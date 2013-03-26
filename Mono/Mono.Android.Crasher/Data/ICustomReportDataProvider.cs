using System.Collections.Generic;
using Android.Content;

namespace Mono.Android.Crasher.Data
{
    public interface ICustomReportDataProvider
    {
        /// <summary>
        /// Allows to insert custom data in crush report.
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <returns><see cref="IDictionary{TKey,TValue}"/> with custom data fields.</returns>
        IDictionary<string, string> GetReportData(Context context);
    }
}