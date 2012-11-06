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

        protected override RegistrationManager DoInstantiateRegistrationManager()
        {
            return new SupervisorRegistrationManager();
        }

        protected override void OnFirstRegistrationStepButtonClicked(DriveInfo drive)
        {
            if (drive == null)
                return;

            var user = this.GetCurrentUser();

            try
            {
                if (RegistrationManager.StartRegistration(drive.Name, user.ToString(), this.urlUtils.GetRegistrationCapiPath()))
                    base.ChangeResultlabel("Registration Completed");
            }
            catch (Exception ex)
            {

                base.ChangeResultlabel("Registration failed: " + ex.Message, true); 
            }
            
            
        }

        #endregion
    }
}
