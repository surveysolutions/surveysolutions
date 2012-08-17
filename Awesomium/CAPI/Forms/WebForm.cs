/***************************************************************************
 *  Project: WinFormsSample
 *  File:    WebForm.cs
 *  Version: 1.6.5.0
 *
 *  Copyright ©2012 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Demonstrates rendering an Awesomium WebView to a Windows Forms UI.
 *  In this sample, we simply render on the Form itself. In a real-life
 *  scenario, this should be a custom user control.
 *  
 *  This sample is available for both Awesomium.NET (.NET 4.0) as well
 *  as for Awesomium.Mono (.NET 2.0 & 4.0), for use with Mono on 
 *  all platforms.
 *   
 ***************************************************************************/

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
using Browsing.CAPI.Synchronization;

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

            InitializeComponent();

            this.statusStrip1.Hide();

            var host = new ToolStripControlHost(this.pleaseWait);
            host.Size = this.statusStrip1.Size;
            this.statusStrip1.Items.AddRange(new ToolStripItem[] { host });

            webView.Source = new Uri(Settings.Default.DefaultUrl);
            this.webView.Focus();
        }

        void sync_EndOfSync(object sender, SynchronizationCompletedEvent e)
        {
            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    this.pullToolStripMenuItem.Enabled = true;
                    if (e.ActionType == SyncType.Pull)
                        webView.LoadURL(Settings.Default.DefaultUrl);
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
                this.webView.IsDirtyChanged -= OnIsDirtyChanged;
                this.webView.SelectLocalFiles -= OnSelectLocalFiles;
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


        private void OnIsDirtyChanged(object sender, EventArgs e)
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
        }
        private void OnSelectLocalFiles(object sender, SelectLocalFilesEventArgs e)
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
        #endregion

        private void pushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.ExportQuestionaries();
            }
            catch (Exception ex)
            {
                // MessageBox.Show("Export error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }

        private void pullToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.syncManager.ImportQuestionaries();
            }
            catch (Exception ex)
            {
                // MessageBox.Show("Export error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }
    }
}