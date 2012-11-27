using System;
using System.Linq;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class Browser : Screen
    {
        #region Members

        private bool continueWebNavigation = true;
        private bool isSinglePage = false;
        private string rootPathString = string.Empty;

        #endregion

        #region C-tor

        public Browser(ScreenHolder holder)
            : base(holder, true)
        {
            this.webView = new WebControl();

            InitializeComponent();

            this.webView.BeginNavigation += new BeginNavigationEventHandler(webView_BeginNavigation);
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Navigates to home page when continueWebNavigation set to false
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webView_BeginNavigation(object sender, BeginNavigationEventArgs e)
        {
            if (!this.continueWebNavigation)
                GoHome();
        }

        void homeButton_Click(object sender, System.EventArgs e)
        {
            GoHome();
        }

        #endregion

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

        protected void GoHome()
        {
            this.Holder.NavigateMain();
        }

        #region Progress indication

        void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
            MenuPanel.Visible = string.Compare(e.Url, this.rootPathString, true) == 0;
        }

        ResourceResponse webView_ResourceRequest(object sender, ResourceRequestEventArgs e)
        {
            this.continueWebNavigation = OnResourceRequest(e.Request);

            return null;
        }

        protected virtual bool OnResourceRequest(ResourceRequest resourceRequest)
        {
            return false;
        }

        void webView_LoadCompleted(object sender, EventArgs e)
        {
            this.progressBox.Visible = false;
            if (this.isSinglePage && string.Compare(this.webView.Source.ToString(), this.rootPathString, true) != 0)
                homeButton_Click(sender, e);
        }

        #endregion
    }
}
