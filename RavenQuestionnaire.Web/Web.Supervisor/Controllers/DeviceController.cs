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
    using Questionnaire.Core.Web;

    using Web.Supervisor.Models;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : RegistrationController
    {
        #region Fields

        /// <summary>
        /// Field of deviceRegister
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

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
        public DeviceController(IViewRepository viewRepository, IGlobalInfoProvider globalInfo)
            : base(viewRepository)
        {
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
            ViewBag.ActivePage = MenuItem.Administration;
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
            return this.SaveRegistration(data);
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

            var model = this.GetRegisteredData(supervisorId);
            return Json(model.Items, JsonRequestBehavior.AllowGet);
        }


        protected override Guid GetARPublicKey(RegisterData data)
        {
            return data.RegistrationId;
        }

        #endregion
    }
}