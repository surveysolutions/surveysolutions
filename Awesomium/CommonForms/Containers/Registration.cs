using System;
using System.Diagnostics;
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
    public abstract partial class Registration : Screen, IUsbWatcher
    {
        private bool isFirstPhaseRegistrationPossible;
        private bool isSecondPhaseRegistrationPossible;

        #region C-tor

        public Registration(
            IRequesProcessor requestProcessor,
            IUrlUtils urlUtils,
            ScreenHolder holder,
            bool isRegistrationListVisible,
            string registerFirstPhaseButtonText,
            string registerSecondPhaseButtonText,
            bool isSecondPhaseWork)
            : base(holder, true)
        {
            InitializeComponent();

            this.regContent.Parent = ContentPanel;

            this.regPanel.FirstPhaseButton.Text = registerFirstPhaseButtonText;
            this.regPanel.FirstPhaseButton.Click += registrationButton1stPhase_Click;
            this.regPanel.SecondPhaseButton.Click += registrationButton2ndPhase_Click;

            this.regPanel.UsbChoosen += usb_Click;

            RegistrationManager = DoInstantiateRegistrationManager(requestProcessor, urlUtils, this.regPanel);
            Debug.Assert(RegistrationManager != null);

            RegistrationManager.FirstPhaseAccomplished += new RegistrationCallback(FirstRegistrationPhaseAccomplished);
            RegistrationManager.SecondPhaseAccomplished += new RegistrationCallback(SecondRegistrationPhaseAccomplished);

            this.authorizedGroupBox.Visible = isRegistrationListVisible;
            this.regPanel.SecondPhaseButton.Visible = !isRegistrationListVisible;
        }

        #endregion

        #region Propeties

        protected internal RegistrationManager RegistrationManager { get; private set; }
        protected ListView AuthorizationList { get { return this.authorizationList; } }

        #endregion

        #region Abstract and Virtual

        protected abstract RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider);

        protected abstract string OnGetCurrentRegistrationStatus();

        protected virtual void OnFirstRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            MakeDefaultAccomplishment(true, args);
        }

        protected virtual void OnSecondRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            MakeDefaultAccomplishment(false, args);
        }

        #endregion

        private void MakeDefaultAccomplishment(bool firstPhase, RegistrationCallbackEventArgs args)
        {
            var isPassed = args.IsPassed;

            if (isPassed)
            {
                EnableFirstPhaseRegistration(!firstPhase);
                EnableSecondPhaseRegistration(firstPhase);
                ShowError(string.Empty);
            }
            else
                ShowError(args.Error.Message);

            ShowResult(args.Message, !isPassed);
        }

        #region Helpers

        void FirstRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            try
            {
                OnFirstRegistrationPhaseAccomplished(manager, args);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        void SecondRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            try
            {
                OnSecondRegistrationPhaseAccomplished(manager, args);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void CheckRegistrationChannel()
        {
            string status = string.Empty;

            try
            {
                ShowError("Checking registration status ...");

                // assume sync possibility by default
                this.isFirstPhaseRegistrationPossible = true;
                this.isSecondPhaseRegistrationPossible = true;

                IList<SynchronizationException> issues = this.RegistrationManager.CheckIssues();
                if (issues == null || issues.Count == 0)
                    return;

                SynchronizationException ex = issues.FirstOrDefault<SynchronizationException>(x => x is LocalHosUnreachableException);
                if (ex != null)
                {
                    this.isFirstPhaseRegistrationPossible = false;
                    this.isSecondPhaseRegistrationPossible = false;

                    status += "\n" + ex.Message;

                    return; // fatal
                }

                ex = issues.FirstOrDefault<SynchronizationException>(x => x is UsbUnacceptableException);
                if (ex != null)
                {
                    this.isFirstPhaseRegistrationPossible = false;
                    this.isSecondPhaseRegistrationPossible = false;

                    status += "\n" + ex.Message;

                    return;
                }

            }
            finally
            {
                ShowError(status);
                ShowRegStatus();

                EnableFirstPhaseRegistration(this.isFirstPhaseRegistrationPossible);
                EnableSecondPhaseRegistration(this.isFirstPhaseRegistrationPossible);
            }
        }

        private void EnableFirstPhaseRegistration(bool enable)
        {
            OnEnableFirstPhaseRegistration(enable);
        }

        protected virtual void OnEnableFirstPhaseRegistration(bool enable)
        {
            this.regPanel.EnableRegistration(enable && this.isSecondPhaseRegistrationPossible, true);
        }

        private void EnableSecondPhaseRegistration(bool enable)
        {
            OnEnableSecondPhaseRegistration(enable);
        }

        protected virtual void OnEnableSecondPhaseRegistration(bool enable)
        {
            this.regPanel.EnableRegistration(enable && this.isSecondPhaseRegistrationPossible, false);
        }

        private void ShowRegStatus()
        {
            ShowResult(GetCurrentRegistrationStatus(), false);
        }

        private string GetCurrentRegistrationStatus()
        {
            return OnGetCurrentRegistrationStatus();
        }

        private void ShowError(string error)
        {
            this.regPanel.ShowError(error);
        }

        private void ShowResult(string text, bool error = false)
        {
            this.regPanel.ShowResult(text, error);
        }

        private void CheckRegistractionPossibilities()
        {
            new System.Threading.Thread(CheckRegistrationChannel).Start();
        }

        private void usb_Click(object sender, EventArgs e)
        {
            CheckRegistractionPossibilities();
        }

        private void registrationButton1stPhase_Click(object sender, EventArgs e)
        {
            try
            {
                RegistrationManager.StartRegistration();
            }
            catch (Exception ex)
            {
                ShowError("Registration failed: " + ex.Message);
            }
        }

        private void registrationButton2ndPhase_Click(object sender, EventArgs e)
        {
            try
            {
                RegistrationManager.FinalizeRegistration();
            }
            catch (Exception ex)
            {
                ShowError("Registration failed: " + ex.Message);
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            CheckRegistractionPossibilities();
        }

        protected void SetUsbStatusText(string text)
        {
            this.regPanel.SetUsbStatusText(text);
        }

        #endregion

        #region IUsbWatcher

        public void UpdateUsbList()
        {
            this.regPanel.UpdateUsbStatus();

            CheckRegistractionPossibilities();
        }

        #endregion
    }
}

