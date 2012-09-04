using System;
using System.Linq;
using Browsing.Common.Controls;
using Browsing.Common.Containers;

namespace Browsing.Supervisor.Containers
{
    public partial class SyncChoicePage : Screen
    {
        #region Constructor

        public SyncChoicePage(ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.tableLayoutPanel1.Parent = this.ContentPanel;
        }

        #endregion

        #region Event Handlers

        private void btnHeadQuater_Click(object sender, EventArgs e)
        {
            var hq = this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncHQProcessPage) as SyncHQProcessPage;
            this.Holder.Redirect(hq);
        }

        private void btnTablet_Click(object sender, EventArgs e)
        {
            var capi = this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncCapiProcessPage) as SyncCapiProcessPage;
            this.Holder.Redirect(capi);
        }
        
        #endregion
    }
}
