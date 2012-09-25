using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Common
{
    public class EngineRunner
    {
        private Process _process;

        public void RunEngine(string engninePath, string port)
        {
            string ProgramFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string IIS_EXPRESS = Path.Combine(ProgramFilesX86, "IIS Express\\iisexpress.exe");
            if (!File.Exists(IIS_EXPRESS))
            {
                throw new Exception("Executor not found!");
            }

            StringBuilder arguments = new StringBuilder();
            arguments.Append(String.Format(" /path:\"{0}\"", engninePath));
            arguments.Append(String.Format(@" /port:{0}", port));
            arguments.Append(@" /systray:false");

            _process = new Process();

            _process.StartInfo.FileName = IIS_EXPRESS;
            _process.StartInfo.Arguments = arguments.ToString();
            //_process.StartInfo.RedirectStandardOutput = true; //?? true doesn't work
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            //_process.OutputDataReceived += Write;
            _process.Start();
        }

        /* private void Write(object sender, DataReceivedEventArgs e)
         {
             File.AppendAllText("C:\\temp\\log.txt", e.Data);
         }*/


        public void StopEngine(object sender, EventArgs e)
        {
            if (_process != null && !_process.HasExited)
                _process.Kill();
        }
    }
}
