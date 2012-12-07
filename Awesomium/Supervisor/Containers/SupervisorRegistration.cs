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
using Synchronization.Core.Events;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorRegistration : Browsing.Common.Containers.Registration
    {
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

            //AuthorizationList.Location = new Point(0, 0);
            //AuthorizationList.AutoArrange = true;
            //AuthorizationList.Columns.Add("Device");
            //AuthorizationList.Columns.Add("Authorization date");
            //AuthorizationList.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            //AuthorizationList.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            AuthorizationList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        #region Helpers

        private void UpdateListView(IEnumerable<RegisterData> source)
        {
            AuthorizationList.Items.Clear();
            foreach (var item in source)
                AuthorizationList.Items.Add(new ListViewItem(new string[] { item.Description, item.RegisterDate.ToLocalTime().ToString() }));

            //AuthorizationList.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent | ColumnHeaderAutoResizeStyle.HeaderSize);
            AuthorizationList.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

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
                        var content = RegistrationManager.DeserializeContent<List<RegisterData>>(devices);

                        UpdateListView(content);
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

        protected override void OnFirstRegistrationPhaseAccomplished(RegistrationEventArgs args)
        {
            if (args.IsPassed)
            {
                foreach(var packet in args.Packets)
                    args.AppendResultMessage(string.Format("CAPI device {0} has been authorized", packet.Data.Description));
            }

            base.OnFirstRegistrationPhaseAccomplished(args);

            if (args.IsPassed)
                UpdateAdministrativeContent();
        }

        protected override void OnEnterScreen()
        {
            base.OnEnterScreen();

            UpdateAdministrativeContent();
        }

        #endregion
    }
}
