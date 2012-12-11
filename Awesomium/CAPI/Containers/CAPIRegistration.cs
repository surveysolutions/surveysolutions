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
using Synchronization.Core.Events;

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
            PublicKeySharedUSB = 1,
            PublicKeySharedNET = 2,
            Confirmed = 3
        }

        // todo: define via resources
        private static IDictionary<CAPIRegistration.Phaze, string> RegButtonStatus = new Dictionary<CAPIRegistration.Phaze, string>(){
            {CAPIRegistration.Phaze.NonRegistered, "(Please, register)"},
            {CAPIRegistration.Phaze.PublicKeySharedUSB, "(Finalize ...)"},
            {CAPIRegistration.Phaze.PublicKeySharedNET, "(Finalize ...)"},
            {CAPIRegistration.Phaze.Confirmed, string.Empty},
        };

        private static string _zeroStepRegisteredMessage;
        private static string _firstStepRegisteredMessageUSB;
        private static string _firstStepRegisteredMessageNET;
        private static string _secondStepRegisteredMessage;
        private readonly static string Register1ButtonText = "Request";
        private readonly static string Register2ButtonText = "Accept";

        public CAPIRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, false, Register1ButtonText, Register2ButtonText, true)
        {
            // todo: define via resources
            _firstStepRegisteredMessageUSB = "This CAPI device \'" + Environment.MachineName + "\' passed first registration step.\nTo proceed, please, authorize your request put on USB flash memory\nby supervisor then finalize registration.";
            _firstStepRegisteredMessageNET = "This CAPI device \'" + Environment.MachineName + "\' passed first registration step via net.\nPlease, wait to accept authorization.";
            //_firstStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' passed first registration step.\nTo proceed, please, authorize your request put on USB flash memory\nby supervisor then finalize registration.";
            _zeroStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' should be authorized by your supervisor.\nPlease, press " + Register1ButtonText + " button to send authorization request.";
            _secondStepRegisteredMessage = "This CAPI device \'" + Environment.MachineName + "\' has been authorized by '{0}'.\nIf necessary you may repeat registration process.";

            InitializeComponent();

            SetUsbStatusText("Current registration status");
        }

        private string ZeroStepRegisteredMessage { get { return _zeroStepRegisteredMessage; } }
        private string FirstStepRegisteredMessageUSB { get { return _firstStepRegisteredMessageUSB; } }
        private string FirstStepRegisteredMessageNET { get { return _firstStepRegisteredMessageNET; } }
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

            if (status == Phaze.PublicKeySharedUSB)
                return FirstStepRegisteredMessageUSB;
            if (status == Phaze.PublicKeySharedNET)
                return FirstStepRegisteredMessageNET;
            else if (status == Phaze.Confirmed)
                return SecondStepRegisteredMessage;
            else
                return ZeroStepRegisteredMessage;
        }

        protected override void OnFirstRegistrationPhaseAccomplished(RegistrationEventArgs args)
        {
            if (args.IsPassed)
            {
                System.Diagnostics.Debug.Assert(args.IsFirstPhase);
                System.Diagnostics.Debug.Assert(args.Packets.Count == 1);

                RegistrationStatus = args.Packets.First().Channel == ServicePacketChannel.Usb ? Phaze.PublicKeySharedUSB : Phaze.PublicKeySharedNET;

                args.AppendResultMessage(args.Packets.First().Channel == ServicePacketChannel.Usb ? FirstStepRegisteredMessageUSB : FirstStepRegisteredMessageNET);
            }

            base.OnFirstRegistrationPhaseAccomplished(args);
        }

        protected override void OnSecondRegistrationPhaseAccomplished(RegistrationEventArgs args)
        {
            if (args.IsPassed)
            {
                System.Diagnostics.Debug.Assert(!args.IsFirstPhase);
                System.Diagnostics.Debug.Assert(args.Packets.Count == 1);

                RegistrationStatus = Phaze.Confirmed;

                args.AppendResultMessage(string.Format(_secondStepRegisteredMessage, args.Packets.First().Data.Description));
            }

            base.OnSecondRegistrationPhaseAccomplished(args);
        }

        protected override void OnEnableSecondPhaseRegistration(bool enable)
        {
            base.OnEnableSecondPhaseRegistration(enable &&
                (RegistrationStatus == Phaze.PublicKeySharedUSB) || (RegistrationStatus == Phaze.PublicKeySharedNET));
        }

        #endregion

        internal string CurrentRegistrationStatus
        {
            get
            {
                var status = RegistrationStatus;

                return RegButtonStatus.ContainsKey(status) ? RegButtonStatus[status] : null;
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
