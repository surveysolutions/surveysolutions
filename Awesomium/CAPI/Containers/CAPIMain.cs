using System;

using System.Windows.Forms;
using Awesomium.Core;
using Browsing.CAPI.Registration;
using Synchronization.Core.Registration;
using Common.Utils;
using Synchronization.Core.Interface;
using Browsing.Common.Containers;
using Browsing.Common.Controls;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Main
    {

        private string registrationFirstStep = Properties.Settings.Default.RegistrationStatus;
        private CapiRegistrationManager capiRegistrationManager;
        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IRSACryptoService rsaCryptoService, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, rsaCryptoService, urlUtils, holder)
        {
            InitializeComponent();

            capiRegistrationManager = new CapiRegistrationManager();
            ChangeRegistrationButton(true, "");
            if (this.registrationFirstStep == "First") ChangeRegistrationButton(true, "Finish Registration");
            else if (this.registrationFirstStep == "Second") ChangeRegistrationButton(false, "Registration Completed");
            else ChangeRegistrationButton(true, "Registration");
          
        }

        #region Override Methods
        
        protected override void OnRegistrationClicked(object sender, System.EventArgs e)
        {
            if (String.IsNullOrEmpty(this.registrationFirstStep))
            {
                ChangeRegistrationButton(null, "Finish Registration");
                capiRegistrationManager.RegistrationFirstStep(rsaCryptoService);
                Properties.Settings.Default.RegistrationStatus = "First";
                Properties.Settings.Default.Save();
                registrationFirstStep = "First";
            }
            else
            if(this.registrationFirstStep == "First")
            {
                var res = capiRegistrationManager.RegistrationSecondStep(rsaCryptoService, this.urlUtils.GetRegistrationCapiPath());
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
