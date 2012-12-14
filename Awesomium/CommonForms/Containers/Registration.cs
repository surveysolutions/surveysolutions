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
using Synchronization.Core.Events;
using Synchronization.Core.Registration;
using Synchronization.Core.Interface;

namespace Browsing.Common.Containers
{
    public abstract partial class Registration : Screen, IUsbWatcher
    {
        private class ServiceTimer : Timer
        {
            private bool isBusy;
            private Registration parentRegistration;

            internal ServiceTimer(Registration parentRegistration) : base()
            {
                this.parentRegistration = parentRegistration;
                this.Interval = 5000;
            }

            protected override void OnTick(EventArgs e)
            {
                lock (this)
                {
                    if (this.isBusy)
                        return;

                    this.isBusy = true;

                    base.OnTick(e);

                    try
                    {
                        this.parentRegistration.TreatServiceWatcherTick(this, EventArgs.Empty);
                    }
                    finally
                    {
                        this.isBusy = false;
                    }
                }
            }
        }

        #region Members

        private bool isFirstPhaseRegistrationPossible;
        private bool isSecondPhaseRegistrationPossible;
        private ServiceTimer serviceWatcher;

        #endregion

        #region C-tor

        public Registration(
            IRequestProcessor requestProcessor,
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

            RegistrationManager.FirstPhaseAccomplished += new EventHandler<RegistrationEventArgs>(FirstRegistrationPhaseAccomplished);
            RegistrationManager.SecondPhaseAccomplished += new EventHandler<RegistrationEventArgs>(SecondRegistrationPhaseAccomplished);

            this.authorizedGroupBox.Visible = isRegistrationListVisible;
            this.regPanel.SecondPhaseButton.Visible = !isRegistrationListVisible;

            this.serviceWatcher = new ServiceTimer(this);
        }

        #endregion

        #region Propeties

        protected internal RegistrationManager RegistrationManager { get; private set; }
        protected ListView AuthorizationList { get { return this.authorizationList; } }

        #endregion

        #region Abstract and Virtual

        protected abstract RegistrationManager DoInstantiateRegistrationManager(IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider);

        protected abstract string OnGetCurrentRegistrationStatus();

        protected virtual void OnFirstRegistrationPhaseAccomplished(RegistrationEventArgs args)
        {
            MakeDefaultAccomplishment(true, args);
        }

        protected virtual void OnSecondRegistrationPhaseAccomplished(RegistrationEventArgs args)
        {
            MakeDefaultAccomplishment(false, args);
        }

        #endregion

        #region Helpers

        private void MakeDefaultAccomplishment(bool firstPhase, RegistrationEventArgs args)
        {
            var isPassed = args.IsPassed;

            if (isPassed)
            {
                EnableFirstPhaseRegistration(!firstPhase);
                EnableSecondPhaseRegistration(firstPhase);
            }

            ShowError(args.ErrorMessage);
            ShowResult(args.ResultMessage, !isPassed);
        }

        private void TreatServiceWatcherTick(object sender, EventArgs e)
        {
            RegistrationManager.CollectAuthorizationPackets();
        }

        void FirstRegistrationPhaseAccomplished(object sender, RegistrationEventArgs args)
        {
            try
            {
                OnFirstRegistrationPhaseAccomplished(args);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        void SecondRegistrationPhaseAccomplished(object sender, RegistrationEventArgs args)
        {
            try
            {
                OnSecondRegistrationPhaseAccomplished(args);
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
                this.isFirstPhaseRegistrationPossible = this.isSecondPhaseRegistrationPossible = true;

                IList<ServiceException> issues = this.RegistrationManager.CheckRegIssues();
                if (issues == null || issues.Count == 0)
                    return;

                ServiceException ex = issues.FirstOrDefault<ServiceException>(x => x is LocalHosUnreachableException);
                if (ex != null)
                {
                    this.isFirstPhaseRegistrationPossible = this.isSecondPhaseRegistrationPossible = false;

                    status += "\n" + ex.Message;

                    return; // fatal
                }

                ex = issues.FirstOrDefault<ServiceException>(x => x is NetUnreachableException || x is InactiveNetServiceException);
                if (ex != null)
                {
                    status = ex.Message;

                    ex = issues.FirstOrDefault<ServiceException>(x => x is UsbNotAccessableException);
                    if (ex != null)
                    {
                        this.isFirstPhaseRegistrationPossible = this.isSecondPhaseRegistrationPossible = false;

                        status += "\n" + ex.Message;

                        return; // fatal
                    }
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

        private void Register(bool asFirstPhase)
        {
            try
            {
                RegistrationManager.DoRegistration(asFirstPhase);
            }
            catch (Exception ex)
            {
                ShowError("Registration failed: " + ex.Message);
            }
        }

        private void registrationButton1stPhase_Click(object sender, EventArgs e)
        {
            Register(true);
        }

        private void registrationButton2ndPhase_Click(object sender, EventArgs e)
        {
            Register(false);
        }

        #endregion

        #region Protected Methods

        protected override void OnEnterScreen()
        {
            base.OnEnterScreen();

            CheckRegistractionPossibilities();

            this.serviceWatcher.Start();
        }

        protected override void OnLeaveScreen()
        {
            this.serviceWatcher.Stop();

            base.OnLeaveScreen();
        }

        protected void SetUsbStatusText(string text)
        {
            this.regPanel.SetUsbStatusText(text);
        }

        #endregion

        #region IUsbWatcher

        public void UpdateUsbList(bool driverAvailable)
        {
            this.regPanel.UpdateUsbStatus();

            if(!driverAvailable)
                RegistrationManager.RemoveUsbChannelPackets();

            CheckRegistractionPossibilities();
        }

        #endregion
    }
}

