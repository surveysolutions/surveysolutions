using System;
using System.Text;
using Android.Content;
using Android.Util;
using Java.IO;
using Java.Lang;

namespace Mono.Android.Crasher.Utils
{
    /// <summary>
    /// Creates a file storing a ID on the first application start. This ID can then be used as a identifier 
    /// of this specific application installation.
    /// </summary>
    class Installation
    {
        private static string _sID;
        private const string InstallationFileName = "Crasher-INSTALLATION";
        private static readonly object _locker = new object();

        /// <summary>
        /// Get or create ID of current application installation
        /// </summary>
        /// <param name="context"><see cref="Context"/> for the application being reported.</param>
        /// <returns>ID of current application installation</returns>
        public static string Id(Context context)
        {
            if (_sID == null)
            {
                var installation = new File(context.FilesDir, InstallationFileName);
                try
                {
                    if (!installation.Exists())
                    {
                        lock (_locker)
                        {
                            if (!installation.Exists())
                                WriteInstallationFile(installation);
                        }
                    }
                    _sID = ReadInstallationFile(installation);
                }
                catch (IOException e)
                {
                    Log.Warn(Constants.LOG_TAG, "Couldn't retrieve InstallationId for " + context.PackageName, e);
                    return "Couldn't retrieve InstallationId";
                }
                catch (RuntimeException e)
                {
                    Log.Warn(Constants.LOG_TAG, "Couldn't retrieve InstallationId for " + context.PackageName, e);
                    return "Couldn't retrieve InstallationId";
                }
            }
            return _sID;
        }

        /// <summary>
        /// Reads installation ID from file.
        /// </summary>
        /// <param name="installation"><see cref="File"/> to read ID from</param>
        /// <returns>Application installation ID</returns>
        private static string ReadInstallationFile(File installation)
        {
            var f = new RandomAccessFile(installation, "r");
            var bytes = new byte[(int)f.Length()];
            try
            {
                f.ReadFully(bytes);
            }
            finally
            {
                f.Close();
            }
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Generates and writes ID of application installation to file.
        /// </summary>
        /// <param name="installation"><see cref="File"/> to write ID</param>
        private static void WriteInstallationFile(File installation)
        {
            var o = new FileOutputStream(installation);
            try
            {
                var id = Guid.NewGuid().ToString();
                o.Write(Encoding.UTF8.GetBytes(id));
            }
            finally
            {
                o.Close();
            }
        }
    }
}