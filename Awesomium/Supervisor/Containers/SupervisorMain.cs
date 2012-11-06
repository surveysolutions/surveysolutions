using System.Linq;
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

        #region Override Methods



        protected override void OnCheckRegistrationButton(bool userIsLoggedIn)
        {
            ChangeRegistrationButton(userIsLoggedIn, "");
        }

        protected override void OnSynchronizationClicked(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncChoicePage));
        }

        #endregion
    }
}
