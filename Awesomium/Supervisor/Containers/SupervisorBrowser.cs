using System;
using System.Linq;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Common.Controls;

namespace Browsing.Supervisor.Containers
{
    
    public partial class SupervisorBrowser : Browser
    {
        #region Constructor

        public SupervisorBrowser(WebControl webView, ScreenHolder holder)
            : base(webView, holder)
        {
            InitializeComponent();
        }

        #endregion
    }
}
