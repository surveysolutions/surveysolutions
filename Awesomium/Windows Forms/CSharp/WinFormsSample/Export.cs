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
using Awesomium.Core;

namespace WinFormsSample
{
    internal class Export
    {

        private PleaseWaitForm pleaseWait;
        private string localFilename;
        private WebClient myWebClient = new WebClient();
        public bool isActive()
        {
            return myWebClient.IsBusy;
        }

        public void Start(string drive, WebView webView)
        {

            pleaseWait = new PleaseWaitForm();
            pleaseWait.Show();
            //myWebClient.Headers.Add(HttpRequestHeader.Cookie);
            Uri exportURL = new Uri(Settings.Default.DefaultUrl + "/Synchronizations/Export");

            string filename = string.Format("backup-{0}.zip", DateTime.Now.ToString().Replace("/", "_"));
            filename = filename.Replace(" ", "_");
            filename = filename.Replace(":", "_");
            
            
            myWebClient.Credentials = new NetworkCredential("Admin", "Admin");
            myWebClient.DownloadProgressChanged += (s, e) =>
            {
                pleaseWait.progressBar.Value = e.ProgressPercentage;
            };
            myWebClient.DownloadFileCompleted += (s, e) =>
            {
                pleaseWait.statusLabel.Text = "Export successfully completed";
                
                End();
            };
            
            localFilename = drive + filename;
            
            try
            {
                myWebClient.DownloadFileAsync(exportURL, localFilename);
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
            

            



        }



        public void End()
        {
            //Thread t = new Thread();
            pleaseWait.Close();

        }





        internal void Stop()
        {
            if (myWebClient.IsBusy)
            {
                try
                {
                    myWebClient.CancelAsync();

                    if (File.Exists(localFilename))
                        File.Delete(localFilename);
                }
                catch (Exception ex)
                {
                    
                    throw ex;
                }
                
            }


        }
    }

}
