using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Controls;
using Browsing.Common.Interfaces;
using Common.Utils;
using Synchronization.Core.Errors;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

namespace Browsing.Common.Containers
{
    public abstract partial class Registration : Screen, IUsbWatcher, IUsbProvider
    {
        private bool isRegistrationPossible;

        #region C-tor

        public Registration(IRequesProcessor requestProcessor, IUrlUtils urlUtils,
                            ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.registrationPanel.Parent = ContentPanel;

            this.usbStatusPanel.UsbPressed += usb_Click;

            this.RegistrationManager = DoInstantiateRegistrationManager(requestProcessor, urlUtils, this);

            System.Diagnostics.Debug.Assert(this.RegistrationManager != null);
        }

        #endregion

        #region Propeties

        protected internal RegistrationManager RegistrationManager { get; private set; }

        #endregion

        #region Abstract

        protected abstract RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider);

        protected abstract bool OnRegistrationButtonClicked(out string message);

        protected abstract string OnGetCurrentRegistrationStatus();

        #endregion

        #region Helpers

        private void CheckRegchannel() 
        {
            string status = string.Empty;

            try
            {
                ShowError("Checking registration status ...");

                // assume sync possibility by default
                this.isRegistrationPossible = true;

                IList<SynchronizationException> issues = this.RegistrationManager.CheckIssues();
                if (issues == null || issues.Count == 0)
                    return;

                SynchronizationException ex = issues.FirstOrDefault<SynchronizationException>(x => x is LocalHosUnreachableException);
                if (ex != null)
                {
                    this.isRegistrationPossible = false;

                    status += "\n" + ex.Message;

                    return; // fatal
                }

                ex = issues.FirstOrDefault<SynchronizationException>(x => x is UsbUnacceptableException);
                if (ex != null)
                {
                    this.isRegistrationPossible = false;

                    status += "\n" + ex.Message;

                    return;
                }

            }
            finally
            {
                ShowError(status);
                ShowRegStatus();

                EnableRegistration(this.isRegistrationPossible);
            }
        }

        private void ShowRegStatus()
        {
            ShowResult(GetCurrentRegistrationStatus(), false);
        }

        private string GetCurrentRegistrationStatus()
        {
            return OnGetCurrentRegistrationStatus();
        }

        private void ShowError(string status)
        {
            this.usbStatusPanel.SetStatus(status, true);
        }

        private void ShowResult(string text, bool error = false)
        {
            this.usbStatusPanel.SetResult(text, error);
        }

        private void EnableRegistration(bool enable)
        {
            if (this.registrationButton.InvokeRequired)
                this.registrationButton.Invoke(new MethodInvoker(() => this.registrationButton.Enabled = enable));
            else
                this.registrationButton.Enabled = enable;
        }

        private void CheckRegistractionPossibilities(bool background = true)
        {
            if (background)
                new System.Threading.Thread(CheckRegchannel).Start();
            else
                CheckRegchannel();
        }

        private void usb_Click(object sender, EventArgs e)
        {
            try
            {
                CheckRegistractionPossibilities();
            }
            catch
            {
            }
        }

        private void registrationButton_Click(object sender, EventArgs e)
        {
            string statusMessage = "Registration failed";
            bool isError = true;

            try
            {
                isError = OnRegistrationButtonClicked(out statusMessage);
            }
            catch (Exception ex)
            {
                statusMessage = "Registration failed: " + ex.Message;
            }
            finally
            {
                ShowResult(statusMessage, isError);
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            CheckRegistractionPossibilities();
        }

        #endregion

        #region IUsbWatcher

        public void UpdateUsbList()
        {
            this.usbStatusPanel.UpdateUsbList();

            CheckRegistractionPossibilities();
        }

        #endregion

        #region IUsbProvider Memebers

        public DriveInfo ActiveUsb
        {
            get { return this.usbStatusPanel.ChosenUsb; }
        }

        public bool IsAnyAvailable
        {
            get { return usbStatusPanel.ReviewDriversList().Count > 0; }
        }

        #endregion
    }
}

