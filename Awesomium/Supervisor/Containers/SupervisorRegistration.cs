using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Browsing.Common.Controls;
using Browsing.Supervisor.Registration;
using Common.Utils;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorRegistration : Browsing.Common.Containers.Registration
    {
        private IUrlUtils urlUtils;
        private IRequesProcessor requestProcessor;
        private readonly static string RegisterButtonText = "Authorize";

        public SupervisorRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, true, RegisterButtonText, string.Empty, false)
        {
            InitializeComponent();

            SetUsbStatusText("CAPI device authorization status");

            this.urlUtils = urlUtils;
            this.requestProcessor = requestProcessor;
        }

        #region Helpers

        private void UpdateAdministrativeContent()
        {
            //var regDevicesList = this.urlUtils.GetRegisteredDevicesUrl();
            //this.requestProcessor.
        }

        #endregion

        #region Override Methods

        protected override string OnGetCurrentRegistrationStatus()
        {
            return string.Empty;
        }

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            return new SupervisorRegistrationManager(requestProcessor, urlUtils, usbProvider);
        }

        protected override void OnEnableSecondPhaseRegistration(bool enable)
        {
            base.OnEnableSecondPhaseRegistration(false);
        }

        protected override void OnFirstRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            if (args.IsPassed)
                args.AppendMessage(string.Format("CAPI device {0} has been authorized", args.Data.Description));

            base.OnFirstRegistrationPhaseAccomplished(manager, args);

            if(args.IsPassed)
                UpdateAdministrativeContent();
        }


        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            UpdateAdministrativeContent();
        }
        #endregion
    }
}
