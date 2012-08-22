using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Awesomium.Core;
using Browsing.CAPI.Properties;
using Synchronization.Core;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Browsing.CAPI.Forms;


namespace Browsing.CAPI.Synchronization
{
    /// <summary>
    /// The class is responsible for import/export operations by CAPI
    /// </summary>
    public class CapiSyncManager : SyncManager
    {
        #region C-tor

        public CapiSyncManager(ISyncProgressObserver pleaseWait, ISettingsProvider provider)
            : base(pleaseWait, provider)
        {
        }

        #endregion

        protected override void OnAddSynchronizers(IList<ISynchronizer> syncChain, ISettingsProvider settingsProvider)
        {
            syncChain.Add(new NetworkSynchronizer(settingsProvider, Settings.Default.DefaultUrl,
                                                                      Settings.Default.NetworkLocalExportPath,
                                                                      Settings.Default.NetworkLocalImportPath,
                                                                      Settings.Default.NetworkCheckStatePath,
                                                                      Settings.Default.EndpointExportPath));

            syncChain.Add(new UsbSynchronizer(settingsProvider, Settings.Default.DefaultUrl,
                                                                  Settings.Default.UsbExportPath,
                                                                  Settings.Default.UsbImportPath));
        }

        #region Helpers

        private void DoExport()
        {
            Push(SyncDirection.Up);
        }

        private void DoImport()
        {
            Pull(SyncDirection.Down);
        }

        #endregion

        #region Overloaded
        protected override void CheckPushPrerequisites()
        {
            bool result = false;
            try
            {
                WebRequest request = WebRequest.Create(new Uri(string.Format("{0}{1}", Settings.Default.DefaultUrl, Settings.Default.CheckEventPath)));
                request.Method = "GET";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    try
                    {
                        result = Convert.ToBoolean(responseFromServer);
                      
                    }
                    finally
                    {
                        // Clean up the streams.
                        reader.Close();
                        dataStream.Close();
                        response.Close();
                    }
                }
            }
            catch (Exception e)
            {
                throw new CheckPrerequisitesException("Check Prerequisites procedure is failed",SyncType.Push, e);
            }
            if (!result)
                throw new CheckPrerequisitesException("Current device don't have any changes", SyncType.Push, null);
        }

        protected override void CheckPullPrerequisites()
        {
            // Prerequisites empty at this moment
        }

        protected override string OnDoSynchronizationAction(SyncType action, SyncDirection direction)
        {
            string syncResult = null;
            try
            {
                syncResult = base.OnDoSynchronizationAction(action, direction);
            }
            catch (Exception e)
            {
                //syncResult = e.Message;
                throw e;
            }
            /*finally
            {
                if (!string.IsNullOrEmpty(syncResult))
                    MessageBox.Show(syncResult);
            }*/

            return syncResult;
        }

        #endregion

        #region Methods

        internal void ExportQuestionaries()
        {
            new Thread(DoExport).Start(); // initialize export operation in independent thread
        }

        internal void ImportQuestionaries()
        {
            new Thread(DoImport).Start(); // initialize export operation in independent thread
        }

        #endregion
    }
}
