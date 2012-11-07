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
            : base(requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
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
            if (RegistrationManager.StartRegistration())
            {
                statusMessage = "CAPI device has been registered";
                return true;
            }

            statusMessage = "Registration of CAPI device has failed";
            return false;
        }

        #endregion
    }
}
