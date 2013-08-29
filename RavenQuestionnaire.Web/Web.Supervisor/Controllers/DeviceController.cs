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

    public class DeviceController : RegistrationController
    {
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IViewFactory<DeviceViewInputModel, DeviceView> deviceViewFactory;

        public DeviceController(IGlobalInfoProvider globalInfo,
            IViewFactory<DeviceItemViewInputModel, DeviceItemView> deviceItemViewFactory,
            IViewFactory<DeviceViewInputModel, DeviceView> deviceViewFactory)
            : base(deviceItemViewFactory, deviceViewFactory)
        {
            this.globalInfo = globalInfo;
            this.deviceViewFactory = deviceViewFactory;
        }

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
            var model = this.deviceViewFactory.Load(new DeviceViewInputModel(user.Id));
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