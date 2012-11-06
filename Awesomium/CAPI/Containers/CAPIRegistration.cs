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

        #region Override Methods

        protected override RegistrationManager DoInstantiateRegistrationManager()
        {
            return new CapiRegistrationManager();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (registrationFirstStep.Equals("First"))
                base.ChangeResultlabel("First registration step complited");
            else 
                if (registrationFirstStep.Equals("Second"))
                    base.ChangeResultlabel("Second registration step complited");
        }

        protected override void OnFirstRegistrationStepButtonClicked(DriveInfo drive)
        {
            if (drive == null)
                return;

            if (String.IsNullOrEmpty(this.registrationFirstStep) || registrationFirstStep.Equals("Second"))
            {
                try
                {
                     if (this.RegistrationManager.StartRegistration(drive.Name))
                         base.ChangeResultlabel("First registration step complited");
                }
                catch (Exception ex)
                {
                    base.ChangeResultlabel("Registration failed: "+ex.Message, true);
                    return;
                }

                Properties.Settings.Default.RegistrationStatus = "First";
                Properties.Settings.Default.Save();
                registrationFirstStep = "First";

               
            }
            else if (registrationFirstStep.Equals("First"))
            {
                try
                {
                    var res = RegistrationManager.FinalizeRegistration(drive.Name,
                                                                       this.urlUtils.GetRegistrationCapiPath());
                    if (res)
                    {
                        base.ChangeResultlabel("Registration Completed");
                        Properties.Settings.Default.RegistrationStatus = "Second";
                        Properties.Settings.Default.Save();
                        registrationFirstStep = "Second";
                    }
                    else
                    {
                        base.ChangeResultlabel("Registration Failed", true);
                    }
                }
                catch(Exception ex)
                {
                    base.ChangeResultlabel("Registration failed: " + ex.Message, true);
                    return;
                }
            }
        }

        #endregion

    }
}
