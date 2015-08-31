using System.Collections.Generic;
using System.IO;
using Android.Util;
using Process = Android.OS.Process;
using System.Text;

namespace Mono.Android.Crasher.Data.Collectors
{
    /// <summary>
    /// Collects results of the dumpsys command.
    /// </summary>
    static class DumpSysCollector
    {
        /// <summary>
        /// Collect results of the dumpsys meminfo command restricted to this application process.
        /// </summary>
        /// <returns>The execution result.</returns>
        public static string CollectMemInfo()
        {
            var meminfo = new StringBuilder();
            try
            {
                var commandLine = new List<string> { "dumpsys", "meminfo", Process.MyPid().ToString() };
                using (var process = Java.Lang.Runtime.GetRuntime().Exec(commandLine.ToArray()))
                {
                    using (var reader = new StreamReader(process.InputStream))
                    {
                        meminfo.Append(reader.ReadToEnd());
                        reader.Close();
                    }
                    process.Destroy();
                }
            }
            catch (IOException e)
            {
                Log.Error(Constants.LOG_TAG, Java.Lang.Throwable.FromException(e), "DumpSysCollector.meminfo could not retrieve data");
            }
            return meminfo.ToString();
        }
    }
}