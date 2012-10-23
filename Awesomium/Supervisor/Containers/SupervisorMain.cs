using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorMain : Main
    {
        #region Constructor

        public SupervisorMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        #endregion

        protected override void AddRegistrationButton(TableLayoutPanel tableLayoutPanel)
        {

        }
        protected override void OnSynchronizationClicked(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncChoicePage));
        }
    }
}
