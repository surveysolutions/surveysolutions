using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Forms;
using WinFormsSample.Properties;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace WinFormsSample
{
    internal class Export
    {
        
        private PleaseWaitForm pleaseWait;
        private string localFilename;
        private WebClient myWebClient;
        public bool isActive()
        {
            return myWebClient.IsBusy;
        }

        public void Start(string drive)
        {

            pleaseWait = new PleaseWaitForm();
            pleaseWait.Show();
            IAsyncResult result;
            Uri exportURL = new Uri(Settings.Default.DefaultUrl+"/Synchronizations/Export");
            
            string filename = string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace("/", "_"));
            filename = filename.Replace(" ", "_");
            filename = filename.Replace(":", "_");
            myWebClient = new WebClient();
            myWebClient.DownloadProgressChanged += (s, e) =>
            {
                pleaseWait.progressBar.Value = e.ProgressPercentage;
            };
            myWebClient.DownloadFileCompleted += (s, e) =>
            {
                pleaseWait.statusLabel.Text = "Export successfully completed";
                Thread.Sleep(10000);
                End();
            };
            
            localFilename = drive + filename;
            myWebClient.DownloadFileAsync(exportURL, localFilename);
            

            


        }

        

        public void End()
        {
            pleaseWait.Close();
  
        }

        

  

        internal void Stop()
        {
            if (myWebClient.IsBusy)
            {
                myWebClient.CancelAsync();
                if (File.Exists(localFilename))
                    File.Delete(localFilename);
            }

            
        }
    }
}
