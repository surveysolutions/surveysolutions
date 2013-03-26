using System.Collections.Generic;
using System.IO;
using Android.Util;
using System.Text;
using Mono.Android.Crasher.Attributes;

namespace Mono.Android.Crasher.Data.Collectors
{
    /// <summary>
    /// Executes logcat commands and collects it's output.
    /// </summary>
    static class LogCatCollector
    {
        /// <summary>
        /// Default number of latest lines kept from the logcat output.
        /// </summary>
        private const int DefaultTailCount = 100;

        /// <summary>
        /// Executes the logcat command with arguments taken from <see cref="CrasherAttribute.LogcatArguments"/>.
        /// </summary>
        /// <param name="bufferName">The name of the buffer to be read: "main" (default), "radio" or "events".</param>
        /// <returns>A string containing the latest lines of the output.</returns>
        public static string CollectLogCat(string bufferName)
        {
            var commandLine = new List<string> { "logcat" };
            if (bufferName != null)
            {
                commandLine.Add("-b");
                commandLine.Add(bufferName);
            }

            int tailCount;
            var logcatArgumentsList = new List<string>(CrashManager.Config.LogcatArguments);
            var tailIndex = logcatArgumentsList.IndexOf("-t");
            if (tailIndex > -1 && tailIndex < logcatArgumentsList.Count)
            {
                tailCount = int.Parse(logcatArgumentsList[tailIndex + 1]);
                if (Compatibility.ApiLevel < 8)
                {
                    logcatArgumentsList.RemoveAt(tailIndex + 1);
                    logcatArgumentsList.RemoveAt(tailIndex);
                    logcatArgumentsList.Add("-d");
                }
            }
            else
            {
                tailCount = -1;
            }

            var logcatBuf = new StringBuilder(tailCount > 0 ? tailCount : DefaultTailCount);
            commandLine.AddRange(logcatArgumentsList);
            try
            {
                using (var process = Java.Lang.Runtime.GetRuntime().Exec(commandLine.ToArray()))
                {
                    using (var reader = new StreamReader(process.InputStream))
                    {
                        Log.Debug(Constants.LOG_TAG, "Retrieving logcat output...");
                        logcatBuf.AppendLine(reader.ReadToEnd());
                        reader.Close();
                    }
                    process.Destroy();
                }
            }
            catch (IOException e)
            {
                Log.Error(Constants.LOG_TAG, Java.Lang.Throwable.FromException(e), "LogCatCollector.collectLogCat could not retrieve data.");
            }
            return logcatBuf.ToString();
        }
    }
}