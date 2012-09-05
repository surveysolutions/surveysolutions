using Awesomium.Core;
using System.Diagnostics;
using System.Windows.Forms;
using Browsing.Supervisor.Forms;
using System.Threading;
using System;
using System.Runtime.InteropServices;


namespace Supervisor
{
    internal static class Program
    {
      

        private static void Main(string[] args)
        {
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, "Supervisor", out createdNew))
            {
               if (!createdNew)
               {
                   MessageBox.Show("One copy of the application is already running");
                   return;
               }
               else
                {
                    // Checks if this is a child rendering process and if so,
                    // transfers control of the process to Awesomium.
                    if (WebCore.IsChildProcess)
                    {
                        WebCore.ChildProcessMain();
                        // When our process is not used any more, exit it.
                        return;
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Using our executable as a child rendering process, is not
                    // available when debugging in VS.
                    if (!Process.GetCurrentProcess().ProcessName.EndsWith("vshost"))
                    {
                        // Initialize the WebCore specifying that this executable
                        // can be used as a child rendering process.
                        WebCore.Initialize(new WebCoreConfig()
                                               {
                                                   ChildProcessPath = WebCoreConfig.CHILD_PROCESS_SELF,
                                                   LogLevel = LogLevel.Verbose,
                                               });
                    }

                    Application.Run(new WebForm());
                }
                
            }
        }
    }
}
