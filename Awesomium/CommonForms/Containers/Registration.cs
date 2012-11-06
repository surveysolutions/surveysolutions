using System;
using System.IO;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Registration;

namespace Browsing.Common.Containers
{
    public abstract partial class Registration : Screen
    {
        #region Fields

        protected internal RegistrationManager RegistrationManager { get; private set; }

        #endregion

        public Registration(IRequesProcessor requestProcessor, IUrlUtils urlUtils,
                            ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.RegistrationManager = DoInstantiateRegistrationManager(requestProcessor, urlUtils);

            System.Diagnostics.Debug.Assert(this.RegistrationManager != null);
        }

        #region Virtual and Abstract

        protected abstract RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils);

        protected abstract bool OnRegistrationButtonClicked(DriveInfo currentDrive, out string message);

        #endregion

        #region Helpers

        private void registrationButton_Click(object sender, EventArgs e)
        {
            string statusMessage = "Registration failed";
            bool isError = true;

            try
            {
                DriveInfo driver = usbStatusPanel.ChosenUsb;

                if (driver == null)
                {
                    statusMessage = "USB driver is not available";
                    return;
                }

                isError = OnRegistrationButtonClicked(driver, out statusMessage);
            }
            catch (Exception ex)
            {
                statusMessage = "Registration failed: " + ex.Message;
            }
            finally
            {
                OutputResult(statusMessage, isError);
            }
        }

        #endregion

        #region Protected Methods

        protected void OutputResult(string text, bool error = false)
        {
            this.usbStatusPanel.SetResult(text, error);
        }

        protected void OutputRegistrationInfo(string[] devices)
        {
        }

        #endregion
    }
}

