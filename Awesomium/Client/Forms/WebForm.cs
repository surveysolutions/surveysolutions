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
using Client.Properties;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;

#endif
#endregion

namespace Client
{
    public partial class WebForm : Form
    {
        #region Fields
        WebView webView;
        RenderBuffer rBuffer;
        Bitmap frameBuffer;
        bool needsResize, repaint;
        private PleaseWaitControl pleaseWait;
        private Export export;
        MenuItem exportItem;
        #endregion

        #region C-tor

        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.

            this.pleaseWait = new PleaseWaitControl();
            this.export = new Export(pleaseWait);

            InitializeComponent();

            this.statusStrip1.Hide();

            //this.Menu = new MainMenu(new MenuItem[]{this.exportItem});

            var host = new ToolStripControlHost(this.pleaseWait);
            host.Size = this.statusStrip1.Size;
            this.statusStrip1.Items.AddRange(new ToolStripItem[] { host});

            this.webView = WebCore.CreateWebView(this.ClientSize.Width, this.ClientSize.Height);
            this.webView.ResizeComplete += OnResizeComplete;
            this.webView.IsDirtyChanged += OnIsDirtyChanged;
            this.webView.SelectLocalFiles += OnSelectLocalFiles;
            this.webView.CursorChanged += OnCursorChanged;
            this.webView.OpenExternalLink += OnOpenLink;
            //this.webView.DomReady += OnDOMReady;
            //this.webView.KeyboardFocusChanged += OnKeyboardFocus;
            this.webView.LoadURL(Settings.Default.DefaultUrl);

            this.webView.Focus();
        }
        #endregion

        #region Methods
        private void ResizeView()
        {
            if ((this.webView == null) || !this.webView.IsLive)
                return;

            if (this.needsResize && !this.webView.IsResizing)
            {
                // Queue an asynchronous resize.
                this.webView.Resize(this.ClientSize.Width, this.ClientSize.Height);
                this.needsResize = false;
            }
        }

        const int WM_DEVICECHANGE = 0x219;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:

                    int n = (int)m.WParam;

                    if (n == 0x8000)
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

                        this.Menu = new MainMenu(new MenuItem[]{this.exportItem});
                        this.exportItem.Enabled = true;
                    }
                    else if (n == 0x8004)
                    {
                        this.exportItem.Enabled = false;
                        this.export.Interrupt();
                        this.export.FlushDriversList();

                        this.Menu = null;
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

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (!this.webView.IsLive)
                return;

            this.webView.Unfocus();
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

        protected override void OnPaint(PaintEventArgs e)
        {
            if ((this.webView != null) && this.webView.IsLive && this.webView.IsDirty)
                this.rBuffer = this.webView.Render();

            if (this.rBuffer != null)
                Utilities.DrawBuffer(this.rBuffer, e.Graphics, this.BackColor, ref this.frameBuffer);
            else
                base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if ((this.webView == null) || !this.webView.IsLive)
                return;

            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                this.needsResize = true;

            // Request resize, if needed.
            this.ResizeView();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectKeyboardEvent(e.GetKeyboardEvent());
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectKeyboardEvent(e.GetKeyboardEvent(WebKeyType.KeyDown));
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectKeyboardEvent(e.GetKeyboardEvent(WebKeyType.KeyUp));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectMouseDown(MouseButton.Left);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectMouseUp(MouseButton.Left);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectMouseMove(e.X, e.Y);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!this.webView.IsLive)
                return;

            this.webView.InjectMouseWheel(e.Delta);
        }
        #endregion

        #region Event Handlers
        private void OnResizeComplete(object sender, ResizeEventArgs e)
        {
            if (!this.webView.IsLive)
                return;

            if (this.needsResize)
                this.ResizeView(); // Process pending resizing.

            // An IsDirtyChanged will normally be called
            // after resizing. Ask for a full invalidation.
            this.repaint = true;
        }

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

        private void OnCursorChanged(object sender, ChangeCursorEventArgs e)
        {
            this.Cursor = Utilities.GetCursor(e.CursorType);
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

        private void OnOpenLink(object sender, OpenExternalLinkEventArgs e)
        {
            if (!this.webView.IsLive)
                return;

            // For this sample, we load external links
            // in the same view.
            this.webView.LoadURL(e.Url);
        }
        #endregion
    }
}