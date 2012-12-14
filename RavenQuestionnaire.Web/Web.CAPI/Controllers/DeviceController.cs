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

namespace Web.CAPI.Controllers
{
    using System.Linq;

    using Questionnaire.Core.Web.Register;
    using Main.Core.Entities;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : Controller
    {
        #region Fields

        /// <summary>
        /// Field Register
        /// </summary>
        private readonly IDeviceRegistry deviceRegister;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="deviceRegister">
        /// The device register.
        /// </param>
        public DeviceController(IDeviceRegistry deviceRegister)
        {
            this.deviceRegister = deviceRegister;
        }

        #endregion

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
            return this.deviceRegister.SaveRegistration(data);
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
            var model = this.deviceRegister.GetRegisteredData(tabletId);
            return Json(model.Items, JsonRequestBehavior.AllowGet);
        }

        public bool AuthorizeBySupervisor(string url, RegisterData data)
        {
            return this.deviceRegister.AuthorizeByRemoteRegistrator(url, data);
        }

        public ActionResult CheckConfirmedAuthorizationStatus(string url, Guid id)
        {
            var data = this.deviceRegister.CheckRemoteAuthorization(url, id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
