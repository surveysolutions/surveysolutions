using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Browsing.Common.Interfaces;
using Common.Utils;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class RegPanel : UserControl, IUsbProvider
    {
        internal EventHandler UsbChoosen;

        internal FlatButton FirstPhaseButton { get { return this.registrationButtonFirstPhase; } }
        internal FlatButton SecondPhaseButton { get { return this.registrationButtonSecondPhase; } }

        #region C-tor

        public RegPanel()
        {
            InitializeComponent();

            this.usbStatusPanel.UsbPressed += usb_Click;
        }

        #endregion

        #region Handlers

        private void usb_Click(object sender, EventArgs e)
        {
            if (UsbChoosen != null)
                UsbChoosen(this, EventArgs.Empty);
        }

        #endregion

        #region Protected Methods

        /*protected override void OnValidateContent()
        {
            base.OnValidateContent();

            CheckRegistractionPossibilities();
        }*/

        internal void SetUsbStatusText(string text)
        {
            this.usbStatusPanel.SetGroupText(text);
        }

        #endregion

        #region Internal Methods

        internal void EnableRegistration(bool enable, bool firstPhase /*second phase otherwise*/)
        {
            var button = firstPhase ? this.registrationButtonFirstPhase : this.registrationButtonSecondPhase;

            if (button.InvokeRequired)
                button.Invoke(new MethodInvoker(() => button.Enabled = enable));
            else
                button.Enabled = enable;
        }

        internal void UpdateUsbStatus()
        {
            this.usbStatusPanel.UpdateUsbList();
        }

        internal void ShowError(string status)
        {
            this.usbStatusPanel.SetStatus(status, true);
        }

        internal void ShowResult(string text, bool error = false)
        {
            this.usbStatusPanel.SetResult(text, error);
        }

        #endregion

        #region IUsbProvider Memebers

        public DriveInfo ActiveUsb
        {
            get { return this.usbStatusPanel.ChosenUsb; }
        }

        public bool IsAnyAvailable
        {
            get { return this.usbStatusPanel.ReviewDriversList().Count > 0; }
        }

        #endregion
    }
}

