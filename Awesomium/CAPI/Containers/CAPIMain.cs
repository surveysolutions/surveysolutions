using System;
using Browsing.CAPI.Registration;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Main
    {

        private string registrationFirstStep = Properties.Settings.Default.RegistrationStatus;
        private CapiRegistrationManager capiRegistrationManager = new CapiRegistrationManager();

        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();

            if (this.registrationFirstStep == "First")
                ChangeRegistrationButton(true, "Finish Registration");
            else if (this.registrationFirstStep == "Second")
                ChangeRegistrationButton(false, "Registration Completed");
            else
                ChangeRegistrationButton(true, "Registration");

        }

        #region Override Methods

        protected override void OnRegistrationClicked(object sender, System.EventArgs e)
        {
            var drive = GetUsbDrive();
            if (drive == null)
                return;

            if (String.IsNullOrEmpty(this.registrationFirstStep))
            {
                if (!capiRegistrationManager.StartRegistration(drive.Name))
                    return;
                
                Properties.Settings.Default.RegistrationStatus = "First";
                Properties.Settings.Default.Save();
                registrationFirstStep = "First";

                ChangeRegistrationButton(true, "Finish Registration");
            }
            else if (this.registrationFirstStep == "First")
            {
                var res = capiRegistrationManager.FinalizeRegistration(drive.Name, this.urlUtils.GetRegistrationCapiPath());
                if (res)
                {
                    ChangeRegistrationButton(false, "Registration Completed");
                    Properties.Settings.Default.RegistrationStatus = "Second";
                    Properties.Settings.Default.Save();
                    registrationFirstStep = "Second";
                }
            }
        }

        #endregion
    }
}
