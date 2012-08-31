using System;
using System.Linq;
using Browsing.Supervisor.Controls;

namespace Browsing.Supervisor.Containers
{
    
    public partial class SynchronizationPage : Screen
    {

        #region Constructor

        public SynchronizationPage(ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();
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
