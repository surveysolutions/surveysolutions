using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Client.ExportEvent;
using Client.Properties;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Awesomium.Core;
using Synchronization.Core;
using Synchronization.Core.ClientSettings;
using Synchronization.Core.SynchronizationFlow;
using SynchronizationEvent = Synchronization.Core.SynchronizationEvent;

namespace Client
{
  //  public delegate void EndOfExport();
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

        public event EventHandler<SynchronizationCompletedEvent> EndOfExport;

        private void DoExport()
        {
            DoSyncronizationAction(SyncType.Push);
        }

        private void DoImport()
        {
            DoSyncronizationAction(SyncType.Pull);
        }
        private void DoSyncronizationAction(SyncType action)
        {
            this.pleaseWait.ActivateExportState();
            try
            {
                IList<Exception> errorList = new List<Exception>();
                this.exportEnded.Reset();
                var succesSynchronizer = this.synchronizer.ExecuteAction((s) =>
                                                                             {
                                                                                 if (action== SyncType.Pull) s.Pull();
                                                                                 else
                                                                                     s.Push();
                                                                             }, errorList);
                this.exportEnded.Set();
                this.pleaseWait.SetCompletedStatus(false, errorList.Count > 0 && succesSynchronizer == null);

                StringBuilder result = new StringBuilder();
                foreach (SynchronizationException synchronizationException in errorList)
                {
                    result.AppendLine(synchronizationException.Message);
                }
                if (succesSynchronizer != null)
                    result.AppendLine(BuildSuccessSyncMessage(succesSynchronizer,action));
                MessageBox.Show(result.ToString());
                if (EndOfExport != null)
                {
                    EndOfExport(this,new SynchronizationCompletedEvent(action));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string BuildSuccessSyncMessage(ISynchronizer synchronizerAgent, SyncType action)
        {
            var usb = synchronizerAgent as UsbSynchronizer;
            if(usb!=null)
            {
                return string.Format("Usb {0} is successful with file {1}", action,
                                     action == SyncType.Pull ? usb.InFilePath : usb.OutFilePath);
            }
            var lan = synchronizerAgent as NetworkSynchronizer;
            if(lan!=null)
            {
                return string.Format("Network {0} is successful with local center {1}",action, lan.Host);
            }
            return string.Format("Synchronization is successful with {0}",
                                 synchronizerAgent.GetType().Name);
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

        internal void Interrupt()
        {
            this.synchronizer.StopAllActions();
        }
        
    }
}
