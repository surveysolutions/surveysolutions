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
    using Main.Core.View;
    using Main.Core.View.Device;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Register;

    using Web.Supervisor.Models;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : Controller
    {
        #region Fields

        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

        /// <summary>
        /// Field of deviceRegister
        /// </summary>
        private readonly IDeviceRegistry deviceRegister;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="register">
        /// The register.
        /// </param>
        /// <param name="viewRepository">
        /// The view Repository.
        /// </param>
        /// <param name="globalInfo">
        /// The global Info.
        /// </param>
        public DeviceController(IDeviceRegistry register, IViewRepository viewRepository, IGlobalInfoProvider globalInfo)
        {
            this.deviceRegister = register;
            this.viewRepository = viewRepository;
            this.globalInfo = globalInfo;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Page with list of registred devices
        /// </summary>
        /// <returns>
        /// Index page
        /// </returns>
        public ActionResult Index()
        {
            var user = this.globalInfo.GetCurrentUser();
            ViewBag.ActivePage = MenuItem.Devices;
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(user.Id));
            return this.View(model);
        }

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