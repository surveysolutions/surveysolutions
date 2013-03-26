using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Preferences;
using Mono.Android.Crasher.Attributes;

namespace Mono.Android.Crasher.Data.Collectors
{
    /// <summary>
    /// Collects data from <see cref="ISharedPreferences"/> files
    /// </summary>
    static class SharedPreferencesCollector
    {
        /// <summary>
        /// Collects data from preferences files setuped in <see cref="CrasherAttribute.AdditionalSharedPreferences"/>
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <returns>>A human readable String containing one key=value pair per line.</returns>
        public static string Collect(Context context)
        {
            var result = new StringBuilder();
            var shrdPrefs = new Dictionary<string, ISharedPreferences> { { "default", PreferenceManager.GetDefaultSharedPreferences(context) } };
            var shrdPrefsIds = CrashManager.Config.AdditionalSharedPreferences;
            if (shrdPrefsIds != null && shrdPrefsIds.Length > 0)
            {
                foreach (var shrdPrefId in shrdPrefsIds)
                {
                    shrdPrefs.Add(shrdPrefId, context.GetSharedPreferences(shrdPrefId, FileCreationMode.Private));
                }
            }

            foreach (var prefsId in shrdPrefs.Keys)
            {
                result.AppendLine(prefsId);
                var prefs = shrdPrefs[prefsId];
                if (prefs != null && prefs.All != null && prefs.All.Count > 0)
                {
                    foreach (var p in prefs.All)
                    {
                        result.AppendFormat("{0}={1}", p.Key, p.Value).AppendLine();
                    }
                }
                else
                {
                    result.AppendLine("null");
                }
                result.AppendLine();
            }
            return result.ToString();
        }
    }
}