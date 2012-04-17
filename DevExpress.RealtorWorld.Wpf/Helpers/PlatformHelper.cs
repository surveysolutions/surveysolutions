using System;
using System.Diagnostics;


namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class PlatformHelper {
        public static void OpenLink(string link) {
            OpenLink(link, "_blank");
        }
        public static void OpenLink(string link, string target) {
            link = link.Trim();
            string[] protocols = new string[] { "http://", "https://" };
            bool needAddProtocol = true;
            foreach(string protocol in protocols) {
                if(link.IndexOf(protocol) == 0) {
                    needAddProtocol = false;
                    break;
                }
            }
            if(needAddProtocol)
                link = protocols[0] + link;
            RunProgram(link, string.Empty, false);
        }
        public static int RunProgram(string program, string args, bool waitOnReturn) {
            Process process = Process.Start(program, args);
            if(waitOnReturn) {
                process.WaitForExit();
                return process.ExitCode;
            }
            return 0;
        }
    }
}
