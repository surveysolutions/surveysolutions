using System;
using System.Linq;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorBrowser : Browser
    {
        #region Constructor

        public SupervisorBrowser(ScreenHolder holder)
            : base(holder)
        {
            InitializeComponent();
        }

        #endregion

        #region Overriden Methods

        protected override bool OnResourceRequest(ResourceRequest resourceRequest)
        {
            Uri uri = new Uri(resourceRequest.Url);

            var continueNavigation = !uri.PathAndQuery.Contains("/Survey/GotoBrowser");
            if (!continueNavigation)
                resourceRequest.Cancel();

            return continueNavigation;
        }

        #endregion
    }
}
