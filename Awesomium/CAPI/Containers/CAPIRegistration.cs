using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Browsing.CAPI.Registration;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIRegistration : Browsing.Common.Containers.Registration
    {
        private string registrationFirstStep = Properties.Settings.Default.RegistrationStatus;

        public CAPIRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        #region Helpers

        private void SaveRegistrationStep(string step)
        {
            Properties.Settings.Default.RegistrationStatus = step;
            Properties.Settings.Default.Save();

            this.registrationFirstStep = step;
        }

        #endregion

        #region Override Methods

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
        {
            return new CapiRegistrationManager(requestProcessor, urlUtils);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (registrationFirstStep.Equals("First"))
                base.OutputResult("First registration step has been completed");
            else if (registrationFirstStep.Equals("Second"))
                base.OutputResult("Second registration step completed");
            else
                base.OutputResult("Please, register your device on supervisor's laptop");
        }

        protected override bool OnRegistrationButtonClicked(DriveInfo drive, out string statusMessage)
        {
            if (String.IsNullOrEmpty(this.registrationFirstStep) || this.registrationFirstStep.Equals("Second"))
            {
                if (this.RegistrationManager.StartRegistration(drive.Name))
                {
                    statusMessage = "First registration step completed";
                    SaveRegistrationStep("First");

                    return true;
                }
            }
            else if (this.registrationFirstStep.Equals("First"))
            {
                if (RegistrationManager.FinalizeRegistration(drive.Name))
                {
                    statusMessage = "Registration Completed";
                    SaveRegistrationStep("Second");

                    return true;
                }
            }

            statusMessage = "Registration failed";
            return false;
        }

        #endregion

    }
}
