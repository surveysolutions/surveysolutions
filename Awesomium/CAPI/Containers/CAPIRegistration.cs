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
            {CAPIRegistration.Phaze.Confirmed, string.Empty},
        };

        private static string _zeroStepRegisteredMessage;
        private static string _firstStepRegisteredMessage;
        private static string _secondStepRegisteredMessage;
        private readonly static string Register1ButtonText = "Request";
        private readonly static string Register2ButtonText = "Accept";

        public CAPIRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, false, Register1ButtonText, Register2ButtonText, true)
        {
            // todo: define via resources
            _firstStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' passed first registration step.\nTo proceed, please, authorize your request put on USB flash memory\nby supervisor then finalize registration.";
            _zeroStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' should be authorized by your supervisor.\nPlease, press " + Register1ButtonText + " button to prepear authorization request on USB flash memory.";
            _secondStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' has been authorized by '{0}'.\nIf necessary you may repeat registration process.";

            InitializeComponent();

            SetUsbStatusText("Current registration status");
        }

        private string ZeroStepRegisteredMessage { get { return _zeroStepRegisteredMessage; } }
        private string FirstStepRegisteredMessage { get { return _firstStepRegisteredMessage; } }
        private string SecondStepRegisteredMessage { get { return string.Format(_secondStepRegisteredMessage, RegisteredSupervisor); } }


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

        protected override void OnFirstRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            if (args.IsPassed)
            {
                RegistrationStatus = Phaze.PublicKeyShared;
                args.AppendMessage(FirstStepRegisteredMessage);
            }

            base.OnFirstRegistrationPhaseAccomplished(manager, args);
        }

        protected override void OnSecondRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            if (args.IsPassed)
            {
                RegistrationStatus = Phaze.Confirmed;
                args.AppendMessage(string.Format(_secondStepRegisteredMessage, args.Data.Description));
            }

            base.OnSecondRegistrationPhaseAccomplished(manager, args);
        }

        #endregion

        internal string CurrentRegistrationStatus
        {
            get
            {
                return RegButtonStatus.ContainsKey(RegistrationStatus) ? RegButtonStatus[RegistrationStatus] : null;
            }
        }

        public string RegisteredSupervisor 
        {
            get
            {
                // TODO: read by service
                return "Supervisor";
            }
        }
    }
}
