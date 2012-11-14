// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeviceController.cs" company="WorldBank">
//   2012
// </copyright>
// <summary>
//   The device controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.Entities;

    using Questionnaire.Core.Web.Register;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : Controller
    {
        #region Fields

        /// <summary>
        /// Field of deviceRegister
        /// </summary>
        private readonly IDeviceRegistry deviceRegister;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="register">
        /// The register.
        /// </param>
        public DeviceController(IDeviceRegistry register)
        {
            this.deviceRegister = register;
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
        public bool RegisterCapi(RegisterData data)
        {
            return this.deviceRegister.SaveRegistrator(data);
        }

        /// <summary>
        /// Select from database publickey of capi
        /// </summary>
        /// <param name="registrator">
        /// The registrator.
        /// </param>
        /// <returns>
        /// Return PublicKey of Capi
        /// </returns>
        public ActionResult GetRegisteredDevices(Guid registrator)
        {
            var model = this.deviceRegister.GetRegisterData(registrator);
            return Json(model.Items, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}