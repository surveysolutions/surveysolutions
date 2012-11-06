using System;
using System.IO;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Registration;

namespace Browsing.Common.Containers
{
    public abstract partial class Registration : Screen
    {
        #region Private Fields

        private IRequesProcessor requestProcessor;
        protected IUrlUtils urlUtils;
        private Guid? currentUser;

        #endregion

        #region Fields

        protected internal RegistrationManager RegistrationManager { get; private set; }

        #endregion

        public Registration(IRequesProcessor requestProcessor, IUrlUtils urlUtils,
                            ScreenHolder holder)
            : base(holder, true)
        {
            InitializeComponent();

            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;

            //this.syncPanel.MakeVisiblePush(false);

            this.RegistrationManager = DoInstantiateRegistrationManager();

            System.Diagnostics.Debug.Assert(this.RegistrationManager != null);

        }

        #region Properties

        private Guid CurrentUser
        {
            get
            {
                if (this.currentUser.HasValue)
                    return this.currentUser.Value;

                this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetCurrentUserGetUrl(), "GET", true, Guid.Empty);

                return this.currentUser.Value;
            }
        }
        #endregion

        #region Virtual and Abstract

        protected abstract RegistrationManager DoInstantiateRegistrationManager();

        protected abstract void OnFirstRegistrationStepButtonClicked(DriveInfo currentDrive);

        #endregion

        #region Helpers

        private void registrationButton_Click(object sender, EventArgs e)
        {
            OnFirstRegistrationStepButtonClicked(usbStatusPanel.ChosenUsb);
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.usbStatusPanel.UpdateLook();


        }

        protected void ChangeStatuslabel(string text, bool error = false)
        {
            this.usbStatusPanel.ChangeStatusLabel(text, error);
        }

        protected void ChangeResultlabel(string text, bool error = false)
        {
            this.usbStatusPanel.ChangeResultLabel(text, error);
        }

        #endregion

        #region Protected Methos

        protected Guid GetCurrentUser()
        {
            return CurrentUser;
        }
        #endregion




    }
}

