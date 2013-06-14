// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceController.cs" company="WorldBank">
//   2012
// </copyright>
// <summary>
//   The device controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web.Mvc;

using Main.Core.View.Device;

namespace Web.CAPI.Controllers
{
    using System.Linq;
    using System.Text;
    using System.Security.Cryptography;

    using Main.Core.Entities;
    using Questionnaire.Core.Web;
    using Main.Core.View;
    using Main.Core.WCF;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.Generic;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : RegistrationController
    {
        private IAuthorizationService channelService = null;

        private string registrarUrl = string.Empty;

        public DeviceController(IViewFactory<DeviceItemViewInputModel, DeviceItemView> deviceItemViewFactory, IViewFactory<DeviceViewInputModel, DeviceView> deviceViewFactory)
            : base(deviceItemViewFactory, deviceViewFactory) {}

        #region Public Methods and Operators

        /// <summary>
        /// Register CAPI device in supervisor db
        /// </summary>
        /// <param name="data">
        /// Register data object
        /// </param>
        /// <returns>
        /// True on success, false otherwise
        /// </returns>
        public bool RegisterSupervisor(RegisterData data)
        {
            return this.SaveRegistration(data);
        }

        /// <summary>
        /// Get Secret Key
        /// </summary>
        /// <param name="registrator">
        /// The registrator.
        /// </param>
        /// <returns>
        /// Return SecretKey
        /// </returns>
        public ActionResult GetAuthoritySupervisor(Guid tabletId)
        {
            var model = this.GetRegisteredData(tabletId);
            return Json(model.Items, JsonRequestBehavior.AllowGet);
        }

        public bool AuthorizeBySupervisor(string url, RegisterData data)
        {
            return this.AuthorizeByRemoteRegistrator(url, data);
        }

        public ActionResult CheckConfirmedAuthorizationStatus(string url, Guid id)
        {
            var data = this.CheckRemoteAuthorization(url, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #region Helpers

        /// <summary>
        /// Lookup supervisor's service by known url
        /// </summary>
        /// <param name="registrarUrl"></param>
        /// <returns></returns>
        private IAuthorizationService LookupSupervisor(string registrarUrl)
        {
            if (this.channelService != null && string.Compare(registrarUrl, this.registrarUrl, true) == 0)
            {
                return this.channelService;
            }

            this.registrarUrl = string.Empty;

            try
            {
                var address = new EndpointAddress(registrarUrl);
                var endpoints = MetadataResolver.Resolve(typeof(IAuthorizationService), address);

                if (endpoints.Count < 1)
                    return null;

                var factory = new ChannelFactory<IAuthorizationService>(endpoints[0].Binding, endpoints[0].Address);

                this.channelService = factory.CreateChannel();
                this.registrarUrl = registrarUrl;

                return this.channelService;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        public bool AuthorizeByRemoteRegistrator(string registrarUrl, RegisterData data)
        {
            var regService = LookupSupervisor(registrarUrl);
            if (regService == null)
                return false;

            try
            {
                return regService.AuthorizeDevice(new AuthorizationPacket() { Data = data });
            }
            catch
            {
                return false;
            }
        }

        public List<RegisterData> CheckRemoteAuthorization(string registrarUrl, Guid registrationId)
        {
            var regService = LookupSupervisor(registrarUrl);
            if (regService == null)
                return null;

            try
            {
                var packets = regService.PickupAuthorizationPackets(registrationId).Packets;
                if (packets.FirstOrDefault() == null)
                    return null;

                var res = new List<RegisterData>();
                foreach (var p in packets)
                    res.Add(p.Data);

                return res;
            }
            catch
            {
                return null;
            }
        }

        protected override Guid GetARPublicKey(RegisterData data)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] bytes = md5Hasher.ComputeHash(data.RegistrationId.ToByteArray());
            return new Guid(bytes);
        }

        #endregion
    }
}
