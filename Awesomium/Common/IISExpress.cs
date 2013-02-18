// -----------------------------------------------------------------------
// <copyright file="IISExpress.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class IISExpress
    {
        internal class NativeMethods
        {
            // Methods
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr GetTopWindow(IntPtr hWnd);
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        }

        public static void SendStopMessageToProcess(int PID)
        {
            try
            {
                for (IntPtr ptr = NativeMethods.GetTopWindow(IntPtr.Zero); ptr != IntPtr.Zero; ptr = NativeMethods.GetWindow(ptr, 2))
                {
                    uint num;
                    NativeMethods.GetWindowThreadProcessId(ptr, out num);
                    if (PID == num)
                    {
                        HandleRef hWnd = new HandleRef(null, ptr);
                        NativeMethods.PostMessage(hWnd, 0x12, IntPtr.Zero, IntPtr.Zero);
                        return;
                    }
                }
            }
            catch (ArgumentException)
            {
            }
        }
        private string IIS_EXPRESS = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "IIS Express\\iisexpress.exe");
        const string CONFIG = "config";
        const string SITE = "site";
        const string APP_POOL = "apppool";
        const string APP_PATH = "path";
        const string APP_PORT = "port";
        Process process;
        IISExpress(string path, string port)
        {
       /*     Config = config;
            Site = site;
            AppPool = apppool;*/

            StringBuilder arguments = new StringBuilder();
            arguments.Append(String.Format(" /{1}:\"{0}\"", path,APP_PATH));
            arguments.Append(String.Format(@" /{1}:{0}", port, APP_PORT));
            arguments.Append(@" /systray:false");

            process = new Process();

            process.StartInfo.FileName = IIS_EXPRESS;
            process.StartInfo.Arguments = arguments.ToString();
            //_process.StartInfo.RedirectStandardOutput = true; //?? true doesn't work
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }
        IISExpress(string config, string site, string apppool)
        {
         /*   Config = config;
            Site = site;
            AppPool = apppool;
            */
            StringBuilder arguments = new StringBuilder();
            if (!string.IsNullOrEmpty(config))
                arguments.AppendFormat("/{0}:{1} ", CONFIG, config);

            if (!string.IsNullOrEmpty(site))
                arguments.AppendFormat("/{0}:{1} ", SITE, site);

            if (!string.IsNullOrEmpty(apppool))
                arguments.AppendFormat("/{0}:{1} ", APP_POOL, apppool);

            process = Process.Start(new ProcessStartInfo()
            {
                FileName = IIS_EXPRESS,
                Arguments = arguments.ToString(),
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
        }

     /*   public string Config { get; protected set; }
        public string Site { get; protected set; }
        public string AppPool { get; protected set; }*/

        public static IISExpress Start(string config, string site, string apppool)
        {
            return new IISExpress(config, site, apppool);
        }
        public static IISExpress Start(string path, string port)
        {
            return new IISExpress(path,port);
        }
        public void Stop()
        {
            SendStopMessageToProcess(process.Id);
            process.Close();
        }
    }
}
