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

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
        {
            return new SupervisorRegistrationManager(requestProcessor, urlUtils);
        }

        protected override bool OnRegistrationButtonClicked(DriveInfo drive, out string statusMessage)
        {
            if (RegistrationManager.StartRegistration(drive.Name))
            {
                statusMessage = "CAPI device has been registered";
                return true;
            }

            statusMessage = "Registration process failed";
            return false;
        }

        #endregion
    }
}
