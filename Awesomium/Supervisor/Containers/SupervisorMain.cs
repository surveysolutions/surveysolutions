using System;
using System.Linq;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Browsing.Supervisor.Registration;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorMain : Main
    {
        private SupervisorRegistrationManager supervisorRegistrationManager = new SupervisorRegistrationManager();

        #region Constructor

        public SupervisorMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        #endregion

        #region Override Methods

        protected override void OnRegistrationClicked(object sender, System.EventArgs e)
        {
            var drive = GetUsbDrive();
            if (drive == null)
                return;

            var user = this.GetCurrentUser();

            supervisorRegistrationManager.StartRegistration(drive.Name, user.ToString(), this.urlUtils.GetRegistrationCapiPath());
        }

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
