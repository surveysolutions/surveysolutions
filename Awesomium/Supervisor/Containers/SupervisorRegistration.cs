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
        private class AuthListViewItem : ListViewItem
        {
            private Color defaultBackColor;
            private Color defaultForeColor;
            private DateTime? pastAuthorization = null;

            private static string DeviceDescriptionContent(IRegisterData data)
            {
                return string.Format("{0} ({1})", data.Description, data.RegistrationId);
            }

            public AuthListViewItem(IRegisterData data)
                : base(new string[] { DeviceDescriptionContent(data), data.RegisterDate.ToLocalTime().ToString() })
            {
                this.defaultForeColor = ForeColor;
                this.defaultBackColor = BackColor;
            }

            public AuthListViewItem(IAuthorizationPacket packet, DateTime? pastAuthorization)
                : this(packet.Data)
            {
                this.pastAuthorization = pastAuthorization;
                Tag = packet;
                UpdateSelection();
            }

            internal void UpdateSelection()
            {
                var packet = Tag as IAuthorizationPacket;
                if (packet == null)
                    return;

                BackColor = Color.Yellow;
                SubItems[1].Text = this.Selected ?
                    (this.pastAuthorization.HasValue ? "Marked to repeat authorization" : "Marked to authorize") :
                    (this.pastAuthorization.HasValue ? this.pastAuthorization.Value.ToLocalTime().ToString() : "Not yet ...");

                packet.MarkToAuthorize(this.Selected);
            }
        }

        private IUrlUtils urlUtils;
        private IRequestProcessor requestProcessor;
        private readonly static string RegisterButtonText = "Authorize";
        private bool isReadingAuthorizationList = false;
        private IList<IAuthorizationPacket> requestPackets = new List<IAuthorizationPacket>();

        public SupervisorRegistration(IRequestProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(requestProcessor, urlUtils, holder, true, RegisterButtonText, string.Empty, false)
        {
            InitializeComponent();

            SetUsbStatusText("CAPI device authorization status");

            this.urlUtils = urlUtils;
            this.requestProcessor = requestProcessor;

            AuthorizationList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            AuthorizationList.MultiSelect = true;
            AuthorizationList.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(AuthorizationList_ItemSelectionChanged);

            RegistrationManager.NewPacketsAvailable += new NewPacketsAvailableHandler(RegistrationManager_NewPacketsAvailable);
        }

        void AuthorizationList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var item = e.Item as AuthListViewItem;
            item.UpdateSelection();
        }

        void RegistrationManager_NewPacketsAvailable(object sender, IList<IAuthorizationPacket> packets)
        {
            lock (this.requestPackets)
            {
                this.requestPackets = this.requestPackets.Union(packets).ToList();
            }

            UpdateAuthorizationList();
        }

        #region Helpers

        private ListViewItem CreateListItem(IAuthorizationPacket packet, DateTime? pastAuthorization)
        {
            return new AuthListViewItem(packet, pastAuthorization);
        }

        private ListViewItem CreateListItem(IRegisterData data)
        {
            return new AuthListViewItem(data);
        }

        private void CleanAuthorizedrequests()
        {
            lock (this.requestPackets)
            {
                this.requestPackets = this.requestPackets.Where(p => !p.IsAuthorized).ToList();
            }
        }

        private void UpdateListView(IEnumerable<RegisterData> source)
        {
            AuthorizationList.Items.Clear();

            Dictionary<Guid, DateTime> registeredDevices = new Dictionary<Guid, DateTime>();

            foreach (var item in source)
            {
                registeredDevices[item.RegistrationId] = item.RegisterDate;

                AuthorizationList.Items.Add(CreateListItem(item));
            }

            foreach (var packet in this.requestPackets)
            {
                DateTime? pastAuthorization = registeredDevices.ContainsKey(packet.Data.RegistrationId) ? registeredDevices[packet.Data.RegistrationId] : (DateTime?)null;
                AuthorizationList.Items.Add(CreateListItem(packet, pastAuthorization));
            }

            AuthorizationList.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
            AuthorizationList.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void UpdateAuthorizationList()
        {
            if (this.isReadingAuthorizationList)
                return;

            CleanAuthorizedrequests();

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

        #endregion

        #region Override Methods

        protected override string OnGetCurrentRegistrationStatus()
        {
            return string.Empty;
        }

        protected override RegistrationManager DoInstantiateRegistrationManager(IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
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
                UpdateAuthorizationList();
        }

        protected override void OnEnterScreen()
        {
            base.OnEnterScreen();

            UpdateAuthorizationList();
        }

        #endregion
    }
}
