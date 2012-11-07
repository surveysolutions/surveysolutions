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
    public partial class CAPIRegistration : Common.Containers.Registration
    {
        /// <summary>
        /// Current registration phaze
        /// </summary>
        internal enum Phaze
        {
            NonRegistered = 0,
            PublicKeyShared = 1,
            Confirmed = 2
        }

        // todo: define via resources
        private static IDictionary<CAPIRegistration.Phaze, string> RegButtonStatus = new Dictionary<CAPIRegistration.Phaze, string>(){
            {CAPIRegistration.Phaze.NonRegistered, "(Please, register)"},
            {CAPIRegistration.Phaze.PublicKeyShared, "(Finalize ...)"},
            {CAPIRegistration.Phaze.Confirmed, null},
        };

        private static string ZeroStepRegisteredMessage;
        private static string FirstStepRegisteredMessage;
        private static string SecondStepRegisteredMessage;

        public CAPIRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder)
        {
            // todo: define via resources
            FirstStepRegisteredMessage = "CAPI device \'" + Environment.MachineName + "\' passed first registration step.\nTo proceed, please, plug USB flash memory\nto supervisor's computer and continue.";
            ZeroStepRegisteredMessage = "CAPI device \'" + Environment.MachineName + "\' should be registered.\nPlease, insert USB flush memory and press Register button.";
            SecondStepRegisteredMessage = "CAPI device \'" + Environment.MachineName + "\' has been registered.\nIf neccesssary you may repeat registration process.";

            InitializeComponent();
        }

        #region Helpers

        private Phaze RegistrationStatus
        {
            get
            {
                return (Phaze)Properties.Settings.Default.RegistrationPhaze;
            }
            set
            {
                Properties.Settings.Default.RegistrationPhaze = (int)value;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Override Methods

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            return new CapiRegistrationManager(requestProcessor, urlUtils, usbProvider);
        }

        protected override string OnGetCurrentRegistrationStatus()
        {
            var status = RegistrationStatus;

            if (status == Phaze.PublicKeyShared)
                return FirstStepRegisteredMessage;
            else if (status == Phaze.Confirmed)
                return SecondStepRegisteredMessage;
            else
                return ZeroStepRegisteredMessage;
        }

        protected override bool OnRegistrationButtonClicked(out string statusMessage)
        {
            var status = RegistrationStatus;

            if (status == Phaze.NonRegistered || status == Phaze.Confirmed)
            {
                if (this.RegistrationManager.StartRegistration())
                {
                    statusMessage = FirstStepRegisteredMessage;
                    RegistrationStatus = Phaze.PublicKeyShared;

                    return true;
                }
            }
            else if (status.Equals("First"))
            {
                if (RegistrationManager.FinalizeRegistration())
                {
                    statusMessage = SecondStepRegisteredMessage;
                    RegistrationStatus = Phaze.Confirmed;

                    return true;
                }
            }

            statusMessage = "Registration failed";
            return false;
        }

        #endregion

        internal string CurrentRegistrationStatus
        {
            get
            {
                return RegButtonStatus.ContainsKey(RegistrationStatus) ? RegButtonStatus[RegistrationStatus] : null;
            }
        }
    }
}
