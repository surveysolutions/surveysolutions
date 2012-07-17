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
using WinFormsSample.Properties;
using System.Net;

#endif
#endregion

namespace WinFormsSample
{
    public partial class WebForm : Form
    {
        #region Fields
        WebView webView;
        RenderBuffer rBuffer;
        Bitmap frameBuffer;
        bool needsResize, repaint;
        #endregion


        #region Ctors


        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.

            InitializeComponent();

            webView = WebCore.CreateWebView(this.ClientSize.Width, this.ClientSize.Height);
            webView.ResizeComplete += OnResizeComplete;
            webView.IsDirtyChanged += OnIsDirtyChanged;
            webView.SelectLocalFiles += OnSelectLocalFiles;
            webView.CursorChanged += OnCursorChanged;
            webView.OpenExternalLink += OnOpenLink;
            //webView.DomReady += OnDOMReady;
            //webView.KeyboardFocusChanged += OnKeyboardFocus;
            webView.LoadURL(Settings.Default.DefaultUrl);

            webView.Focus();
        }
        #endregion


        #region Methods
        private void ResizeView()
        {
            if ((webView == null) || !webView.IsLive)
                return;

            if (needsResize && !webView.IsResizing)
            {
                // Queue an asynchronous resize.
                webView.Resize(this.ClientSize.Width, this.ClientSize.Height);
                needsResize = false;
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!webView.IsLive)
                return;

            webView.Focus();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (!webView.IsLive)
                return;

            webView.Unfocus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (webView != null)
            {
                webView.IsDirtyChanged -= OnIsDirtyChanged;
                webView.SelectLocalFiles -= OnSelectLocalFiles;
                webView.Close();
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
            if ((webView != null) && webView.IsLive && webView.IsDirty)
                rBuffer = webView.Render();

            if (rBuffer != null)
                Utilities.DrawBuffer(rBuffer, e.Graphics, this.BackColor, ref frameBuffer);
            else
                base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if ((webView == null) || !webView.IsLive)
                return;

            if (this.ClientSize.Width > 0 && this.ClientSize.Height > 0)
                needsResize = true;

            // Request resize, if needed.
            this.ResizeView();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (!webView.IsLive)
                return;

            webView.InjectKeyboardEvent(e.GetKeyboardEvent());
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!webView.IsLive)
                return;

            webView.InjectKeyboardEvent(e.GetKeyboardEvent(WebKeyType.KeyDown));
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (!webView.IsLive)
                return;

            webView.InjectKeyboardEvent(e.GetKeyboardEvent(WebKeyType.KeyUp));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!webView.IsLive)
                return;

            webView.InjectMouseDown(MouseButton.Left);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!webView.IsLive)
                return;

            webView.InjectMouseUp(MouseButton.Left);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!webView.IsLive)
                return;

            webView.InjectMouseMove(e.X, e.Y);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!webView.IsLive)
                return;

            webView.InjectMouseWheel(e.Delta);
        }
        #endregion

        #region Event Handlers
        private void OnResizeComplete(object sender, ResizeEventArgs e)
        {
            if (!webView.IsLive)
                return;

            if (needsResize)
                this.ResizeView(); // Process pending resizing.

            // An IsDirtyChanged will normally be called
            // after resizing. Ask for a full invalidation.
            repaint = true;
        }

        private void OnIsDirtyChanged(object sender, EventArgs e)
        {
            if (!webView.IsLive)
                return;

            if (webView.IsDirty)
            {
                // Force repaint.
                if (repaint)
                {
                    // Invalidate the whole surface.
                    this.Invalidate();
                    repaint = false;
                }
                else
                {
                    // Invalidate the dirty region only.
                    // This significantly improves performance.
                    this.Invalidate(webView.DirtyBounds.GetRectangle(), false);
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
            if (!webView.IsLive)
                return;

            // For this sample, we load external links
            // in the same view.
            webView.LoadURL(e.Url);
        }
        #endregion
    }
}