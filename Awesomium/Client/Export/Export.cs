using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Client.Properties;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Awesomium.Core;
using Synchronization.Core;
using Synchronization.Core.ClientSettings;
using Synchronization.Core.SynchronizationFlow;

namespace Client
{
    public delegate void EndOfExport();
    /// <summary>
    /// The class is responsible for completed questionaries export to plugged USB driver
    /// </summary>
    
    internal class Export
    {
        #region Nested Class

        /// <summary>
        /// Helper class to pass neccessary objects into delegate methods of parent class working with WebClient callbacks
        /// </summary>
        private class ProgressHint
        {
            public IStatusIndicator ProgressIndicator { get; private set; }
            public string ArchiveFileName { get; private set; }

            public ProgressHint(IStatusIndicator indicator, string archiveFileName)
            {
                ProgressIndicator = indicator;
                ArchiveFileName = archiveFileName;
            }
        }

        #endregion

        #region Private Members

        internal IStatusIndicator pleaseWait;
       
        private AutoResetEvent exportEnded = new AutoResetEvent(false);
     //   private string exportURL = Settings.Default.DefaultUrl + "/Synchronizations/Export";
        private readonly SyncChainDirector synchronizer;

        #endregion

        #region C-tor

        internal Export(IStatusIndicator pleaseWait, IClientSettingsProvider provider)
        {
            //this.webClient.Credentials = new NetworkCredential("Admin", "Admin");
            this.pleaseWait = pleaseWait;
            this.synchronizer=new SyncChainDirector();
            
            this.synchronizer.PushProgressChanged += (s, e) => this.pleaseWait.AssignProgress(e.ProgressPercentage);
            this.synchronizer.PullProgressChanged += (s, e) => this.pleaseWait.AssignProgress(e.ProgressPercentage);

            this.synchronizer.AddSynchronizer(new NetworkSynchronizer(provider, Settings.Default.DefaultUrl,
                                                                      Settings.Default.NetworkLocalExportPath,
                                                                      Settings.Default.NetworkLocalImportPath,
                                                                      Settings.Default.NetworkCheckStatePath,
                                                                      Settings.Default.EndpointExportPath));
            this.synchronizer.AddSynchronizer(new UsbSynchronizer(provider, Settings.Default.DefaultUrl,
                                                                  Settings.Default.UsbExportPath,
                                                                  Settings.Default.UsbImportPath));

        }

        #endregion

        #region Helpers

        public event EndOfExport EndOfExport;

        private void DoExport()
        {
            DoSyncronizationAction((s) => s.Push());
        }

        private void DoImport()
        {
            DoSyncronizationAction((s)=>s.Pull());
        }
        private void DoSyncronizationAction(Action<ISynchronizer> action)
        {
            this.pleaseWait.ActivateExportState();
            try
            {
                IList<Exception> errorList = new List<Exception>();
                this.exportEnded.Reset();
                var succesSynchronizer = this.synchronizer.ExecuteAction(action, errorList);
                this.exportEnded.Set();
                this.pleaseWait.SetCompletedStatus(false, errorList.Count > 0 && succesSynchronizer == null);

                StringBuilder result = new StringBuilder();
                foreach (SynchronizationException synchronizationException in errorList)
                {
                    result.AppendLine(synchronizationException.Message);
                }
                if (succesSynchronizer != null)
                    result.AppendLine(string.Format("Synchronization is successful with {0}",
                                                    succesSynchronizer.GetType().Name));
                MessageBox.Show(result.ToString());
                if (EndOfExport != null)
                {
                    EndOfExport();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Methods

        internal void ExportQuestionariesArchive()
        {
            new Thread(DoExport).Start(); // initialize export operation in independent thread
        }
        internal void ImportQuestionarie()
        {
            new Thread(DoImport).Start(); // initialize export operation in independent thread
        }
        #endregion

      /*  internal void Interrupt()
        {
            new Thread(Stop).Start();
        }*/
        
    }
}
