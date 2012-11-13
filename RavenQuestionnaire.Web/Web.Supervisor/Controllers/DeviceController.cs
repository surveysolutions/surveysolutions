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
    using System.Linq;
    using System.Web.Mvc;

    using Main.Core.Commands.Synchronization;
    using Main.Core.View;
    using Main.Core.View.Device;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Questionnaire.Core.Web.Register;

    /// <summary>
    /// The device controller.
    /// </summary>
    public class DeviceController : Controller
    {
        #region Fields

        /// <summary>
        /// ViewRepository field
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalProvider;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        public DeviceController(IViewRepository repository, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = repository;
            this.globalProvider = globalProvider;
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
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new RegisterDeviceCommand(data.Description, data.SecretKey, data.TabletId, data.GuidCurrentUser));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Select from database publickey of capi
        /// </summary>
        /// <returns>
        /// Return PublicKey of Capi
        /// </returns>
        public ActionResult GetRegisteredDevices()
        {
            var currentUser = this.globalProvider.GetCurrentUser();
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(Guid.Empty, currentUser.Id));
            return this.PartialView("Devices", model);
        }

        #endregion
    }
}