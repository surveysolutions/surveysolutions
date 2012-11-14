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

using Questionnaire.Core.Web.Register;

namespace Web.CAPI.Controllers
{
    using System.Linq;

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
            return this.deviceRegister.SaveRegistrator(data);
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
        public JsonResult GetRegisteredSupervisors(Guid registrator)
        {
            var model = this.deviceRegister.GetRegisterData(registrator);
            return this.Json(new { PublicKey = model.Items.FirstOrDefault().SecretKey }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
