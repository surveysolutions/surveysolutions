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
        /// <summary>
        /// TODO: Update summary.
        /// </summary>
        public class SyncDeviceRegisterDocument
        {
            #region Fields

            /// <summary>
            /// Gets or sets the public key.
            /// </summary>
            public Guid PublicKey { get; set; }

            /// <summary>
            /// Gets or sets the creation date.
            /// </summary>
            public DateTime CreationDate { get; set; }

            /// <summary>
            /// Gets or sets TabletId.
            /// </summary>
            public Guid TabletId { get; set; }

            /// <summary>
            /// Gets or sets PublicKey.
            /// </summary>
            public byte[] SecretKey { get; set; }

            /// <summary>
            /// Gets or sets Description.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets Registrator.
            /// </summary>
            public Guid Registrator { get; set; }

            #endregion
        }


        private IUrlUtils urlUtils;
        private IRequesProcessor requestProcessor;
        private readonly static string RegisterButtonText = "Authorize";
        private bool isReadingAuthorizationList = false;

        public SupervisorRegistration(IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, true, RegisterButtonText, string.Empty, false)
        {
            InitializeComponent();

            SetUsbStatusText("CAPI device authorization status");

            this.urlUtils = urlUtils;
            this.requestProcessor = requestProcessor;
        }

        #region Helpers

        private void UpdateAuthorizedList()
        {
            if (this.isReadingAuthorizationList)
                return;

            lock (this)
            {
                this.isReadingAuthorizationList = true;

                try
                {
                    var url = this.urlUtils.GetAuthorizedIDsUrl(RegistrationManager.RegistrationId);

                    var devices = this.requestProcessor.Process<string>(url, "False");
                    if (string.Compare(devices, "False", true) != 0)
                    {
                        var content = RegistrationManager.DeserializeContent<List<SyncDeviceRegisterDocument>>(devices);
                    }
                }
                catch
                {
                }
                finally
                {
                    this.isReadingAuthorizationList = false;
                }
            }
        }

        private void UpdateAdministrativeContent()
        {
            UpdateAuthorizedList();
            //new System.Threading.Thread(UpdateAuthorizedList).Start(); // a bug to read in a secondary thread
        }

        #endregion

        #region Override Methods

        protected override string OnGetCurrentRegistrationStatus()
        {
            return string.Empty;
        }

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequesProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
        {
            return new SupervisorRegistrationManager(requestProcessor, urlUtils, usbProvider);
        }

        protected override void OnEnableSecondPhaseRegistration(bool enable)
        {
            base.OnEnableSecondPhaseRegistration(false);
        }

        protected override void OnFirstRegistrationPhaseAccomplished(RegistrationManager manager, RegistrationCallbackEventArgs args)
        {
            if (args.IsPassed)
                args.AppendMessage(string.Format("CAPI device {0} has been authorized", args.Data.Description));

            base.OnFirstRegistrationPhaseAccomplished(manager, args);

            if(args.IsPassed)
                UpdateAdministrativeContent();
        }


        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            UpdateAdministrativeContent();
        }
        #endregion
    }
}
