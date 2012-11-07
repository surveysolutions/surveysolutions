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
        public SupervisorRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, true)
        {
            InitializeComponent();

            SetUsbStatusText("CAPI device registration  status");
        }

        #region Override Methods

        protected override string OnGetCurrentRegistrationStatus()
        {
            return string.Empty;
        }

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            return new SupervisorRegistrationManager(requestProcessor, urlUtils, usbProvider);
        }

        protected override bool OnRegistrationButtonClicked(out string statusMessage)
        {
            RegisterData registeredData;
            if (RegistrationManager.StartRegistration(out registeredData))
            {
                statusMessage = string.Format("CAPI device '{0}' has been registered", registeredData.Description);
                return true;
            }

            statusMessage = "Registration of CAPI device failed";
            return false;
        }

        #endregion
    }
}
