using System;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Browsing.Supervisor.Registration;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorMain : Main
    {
        private string registrationFirstStep = Properties.Settings.Default.RegistrationStatus;
        private SupervisorRegistrationManager supervisorRegistrationManager;
        
        #region Constructor

        public SupervisorMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IRSACryptoService rsaCryptoService, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor,rsaCryptoService, urlUtils, holder)
        {
            InitializeComponent();
            supervisorRegistrationManager = new SupervisorRegistrationManager();

            
            if (this.registrationFirstStep == "First") ChangeRegistrationButton(false, "Registration Completed");
            else ChangeRegistrationButton(true, "Registration");
        }

        #endregion

        #region Override Methods

        protected override void OnRegistrationClicked(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(this.registrationFirstStep))
            {
                var user = this.OnGetCurrentUser();
                supervisorRegistrationManager.RegistrationFirstStep(rsaCryptoService, user.ToString(),
                                                                    this.urlUtils.GetRegistrationCapiPath());

                Properties.Settings.Default.RegistrationStatus = "First";
                Properties.Settings.Default.Save();
                registrationFirstStep = "First";
                ChangeRegistrationButton(false, "Registration Completed");
            }
            else
            {
                ChangeRegistrationButton(false, "Registration Completed");
            }
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
