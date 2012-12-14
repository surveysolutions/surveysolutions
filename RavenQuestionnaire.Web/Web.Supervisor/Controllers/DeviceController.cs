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

    using Questionnaire.Core.Web.Helpers;
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
        private readonly IGlobalInfoProvider globalInfo;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="register">
        /// The register.
        /// </param>
        public DeviceController(IDeviceRegistry register, IGlobalInfoProvider globalInfo)
        {
            this.deviceRegister = register;
            this.globalInfo = globalInfo;
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
            return this.deviceRegister.SaveRegistration(data);
        }

        /// <summary>
        /// Confirms made authorization to CAPI
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>At the moment the only action is to update respective authorization request</remarks>
        public bool ConfirmAuthorization(RegisterData data)
        {
            try
            {
                return Supervisor.WCF.AuthorizationService.ConfirmAuthorizedRequest(data);
            }
            catch 
            { 
            }

            return false;
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
        public ActionResult GetRegisteredDevices(Guid supervisorId)
        {
            //var currentSupervisor = this.globalInfo.GetCurrentUser();
            //System.Diagnostics.Debug.Assert(supervisorId == currentSupervisor.Id);

            var model = this.deviceRegister.GetRegisteredData(supervisorId);
            return Json(model.Items, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}