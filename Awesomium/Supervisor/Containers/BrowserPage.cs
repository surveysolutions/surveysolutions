using System;
using System.Linq;
using Awesomium.Core;
using Awesomium.Windows.Forms;

namespace Browsing.Supervisor.Containers
{
    public partial class BrowserPage : Screen
    {

        #region Properties

        protected bool isSinglePage = false;
        protected string rootPathString = string.Empty;

        #endregion

        #region Constructor

        public BrowserPage(WebControl webView, ScreenHolder holder)
            : base(holder)
        {
            this.webView = webView;

            InitializeComponent();
        }

        #endregion

        #region Methods

        public void SetMode(bool isSinglePageMode, string rootPath)
        {
            this.rootPathString = rootPath;
            this.progressBox.Visible = true;
            this.isSinglePage = isSinglePageMode;
            this.panel1.Visible = true;
            try
            {
                this.webView.Source = new Uri(this.rootPathString);
                this.webView.Focus();
            }
            catch
            {
            }
        }

        private void homeButton_Click(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is MainPage));
        }

        #region TODO Progress indication

        private void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
            this.panel1.Visible = e.Url == this.rootPathString;
        }

        private ResourceResponse webView_ResourceRequest(object sender, ResourceRequestEventArgs e)
        {
            return null;
        }

        private void webView_LoadCompleted(object sender, EventArgs e)
        {
            this.progressBox.Visible = false;
            if (isSinglePage && this.webView.Source.ToString() != this.rootPathString)
                homeButton_Click(sender, e);
        }

        #endregion

        #endregion
        
    }
}
