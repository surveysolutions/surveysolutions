using System;
using System.Linq;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class Browser : Screen
    {
        protected bool isSinglePage = false;
        protected string rootPathString = string.Empty;

        public Browser()
            : base(null, true)
        {
        }

        public Browser(WebControl webView, ScreenHolder holder)
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
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is Main));
        }

        #region Progress indication

        void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
            MenuPanel.Visible = string.Compare(e.Url, this.rootPathString, true) == 0;
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
