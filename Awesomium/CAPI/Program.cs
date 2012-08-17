using System;
using Awesomium.Core;
using System.Diagnostics;
using System.Windows.Forms;
using Browsing.CAPI.Forms;

namespace Browsing.CAPI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main( string[] args )
        {
            // Checks if this is a child rendering process and if so,
            // transfers control of the process to Awesomium.
            if ( WebCore.IsChildProcess )
            {
                WebCore.ChildProcessMain();
                // When our process is not used any more, exit it.
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            // Using our executable as a child rendering process, is not
            // available when debugging in VS.
            if ( !Process.GetCurrentProcess().ProcessName.EndsWith( "vshost" ) )
            {
                // Initialize the WebCore specifying that this executable
                // can be used as a child rendering process.
                WebCore.Initialize( new WebCoreConfig()
                {
                    ChildProcessPath = WebCoreConfig.CHILD_PROCESS_SELF,
                    LogLevel = LogLevel.Verbose,
                } );
            }

            Application.Run( new WebForm() );
        }
    }
}
