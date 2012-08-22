#region Using
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
#if USING_MONO
using Awesomium.Mono;
using Awesomium.Mono.Forms;
#else
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI;
using Browsing.CAPI.Properties;
using Browsing.CAPI.ClientSettings;
using Synchronization.Core.Events;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Browsing.CAPI.Synchronization;
using Common;

#endif
#endregion

namespace Browsing.CAPI.Forms
{
    public partial class WebForm : Form
    {
        #region Fields
        bool repaint;
        private PleaseWaitControl pleaseWait;
        private CapiSyncManager syncManager;
        private ISettingsProvider clientSettings;
        #endregion

        #region C-tor

        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.

            this.pleaseWait = new PleaseWaitControl();
            this.clientSettings = new ClientSettingsProvider();
            this.syncManager = new CapiSyncManager(this.pleaseWait, this.clientSettings);
            this.syncManager.EndOfSync += new EventHandler<SynchronizationCompletedEvent>(sync_EndOfSync);
            this.syncManager.BgnOfSync += new EventHandler<SynchronizationEvent>(sync_BgnOfSync);

            InitializeComponent();

            this.statusStrip1.Hide();
            this.progressBox.Visible = false;

            var host = new ToolStripControlHost(this.pleaseWait);
            host.Size = this.statusStrip1.Size;
            this.statusStrip1.Items.AddRange(new ToolStripItem[] { host });

            this.webView.BeginLoading += new BeginLoadingEventHandler(webView_BeginLoading);
            this.webView.LoadCompleted += new EventHandler(webView_LoadCompleted);
            this.webView.ResourceRequest += new ResourceRequestEventHandler(webView_ResourceRequest);

            string url;
            if (Settings.Default.RunClient)
            {
                try
                {
                    url = RunEngine();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on Engine Run. " + ex.Message);
                    url = Settings.Default.DefaultUrl;
                }

            }
            else
            {
                url = Settings.Default.DefaultUrl;
            }


            try
            {
                this.webView.Source = new Uri(Settings.Default.DefaultUrl);
                this.webView.Focus();
            }
            catch
            {
                // log
            }
        }

        #region TODO Progress indication

        void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
        }

        ResourceResponse webView_ResourceRequest(object sender, ResourceRequestEventArgs e)
        {
            return null;
        }

        void webView_LoadCompleted(object sender, EventArgs e)
        {
            this.progressBox.Visible = false;
        }

        #endregion

        private void EnableDisableMenuItems(bool enable)
        {
            foreach (ToolStripMenuItem item in this.menuStrip1.Items)
            {
                item.Enabled = item == this.toolStripCancelMenuItem ? !enable : enable;
            }
        }

        void sync_EndOfSync(object sender, SynchronizationCompletedEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    MessageBox.Show(this, e.Log);

                    EnableDisableMenuItems(true);

                    if (e.Status.ActionType == SyncType.Pull)
                        webView.LoadURL(Settings.Default.DefaultUrl);
                }));
        }

        void sync_BgnOfSync(object sender, SynchronizationEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    EnableDisableMenuItems(false);
                }));
        }

        #endregion

        #region Methods
        const int WM_DEVICECHANGE = 0x219;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:

                    int n = (int)m.WParam;

                    /* if (n == 0x8000)
                     {
                          //Thread.Sleep(1000);
                         try
                         {
                             this.export.ExportQuestionariesArchive();
                         }
                         catch (Exception ex)
                         {
                             // MessageBox.Show("Export error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                             throw ex;
                         }
                     }
                     else*/
                    if (n == 0x8004)
                    {

                        // this.export.Stop();
                        //  this.export.FlushDriversList();

                        //  this.Menu = null;
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!this.webView.IsLive)
                return;

            this.webView.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (this.webView != null)
            {
                //this.webView.IsDirtyChanged -= OnIsDirtyChanged;
                //this.webView.SelectLocalFiles -= OnSelectLocalFiles;
                this.webView.Close();
            }

            base.OnFormClosed(e);

#if USING_MONO
            // TODO: Mac OS X: Sends a SIGSEGV to Mono.
            if ( !PlatformDetection.IsMac )
#endif
            WebCore.Shutdown();
        }

        #endregion

        #region Event Handlers

        /*private void OnIsDirtyChanged(object sender, EventArgs e)
        {
            if (!this.webView.IsLive)
                return;

            if (this.webView.IsDirty)
            {
                // Force repaint.
                if (this.repaint)
                {
                    // Invalidate the whole surface.
                    this.Invalidate();
                    this.repaint = false;
                }
                else
                {
                    // Invalidate the dirty region only.
                    // This significantly improves performance.
                    this.Invalidate(this.webView.DirtyBounds.GetRectangle(), false);
                }
            }
        }*/

        /*private void OnSelectLocalFiles(object sender, SelectLocalFilesEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = e.Title,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                CheckFileExists = true,
                Multiselect = e.SelectMultipleFiles
            })
            {
                if ((dialog.ShowDialog(this) == DialogResult.OK) || dialog.FileNames.Length > 0)
                    e.SelectedFiles = dialog.FileNames;
                else
                    e.Cancel = true;
            }
        }
         */ 
        #endregion

        private void pushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.ExportQuestionaries();
            }
            catch
            {
            }
        }

        private void pullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.ImportQuestionaries();
            }
            catch
            {
            }
        }

        private void toolStripSettingsMenuItem_Click(object sender, EventArgs e)
        {
            new SettingsBox().ShowDialog();
        }

        private void toolStripCancelMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.Stop();
            }
            catch
            {
            }
        }

        private string RunEngine()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);

            if (dir.Parent == null)
                throw new Exception("Client was not found.");

            string enginePath = Path.Combine(dir.Parent.FullName, Settings.Default.EnginePathName);

            if (!Directory.Exists(enginePath))
                throw new Exception("Client was not found.");

            string port = Settings.Default.DefaultPort;

            EngineRunner runner = new EngineRunner();
            runner.RunEngine(enginePath, port);
            Application.ApplicationExit += runner.StopEngine;

            return String.Format("http://localhost:{0}", port);
        }
    }
}