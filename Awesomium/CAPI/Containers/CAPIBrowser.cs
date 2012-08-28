using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI.Properties;
using Common;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIBrowser : Screen
    {
        protected bool isSinglePage = false;
        protected string rootPathString = string.Empty;
        public CAPIBrowser(WebControl webView, ScreenHolder holder)
            : base(holder, true)
        {
            this.webView = webView;

            InitializeComponent();
        }

        public void SetMode(bool isSinglePageMode, string rootPath)
        {
            this.rootPathString = rootPath;
            this.progressBox.Visible = true;
            this.isSinglePage = isSinglePageMode;

            MenuPanel.Visible = true;
            
            try
            {
                this.webView.Source = new Uri(this.rootPathString);
                this.webView.Focus();
            }
            catch
            {
                // log
            }
        }

        void homeButton_Click(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIMain));
        }

        #region TODO Progress indication

        void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
            MenuPanel.Visible = e.Url == this.rootPathString;
        }

        ResourceResponse webView_ResourceRequest(object sender, ResourceRequestEventArgs e)
        {
            return null;
        }

        void webView_LoadCompleted(object sender, EventArgs e)
        {
            this.progressBox.Visible = false;
            if (isSinglePage && this.webView.Source.ToString() != this.rootPathString)
                homeButton_Click(sender, e);
        }

        #endregion
    }
}
