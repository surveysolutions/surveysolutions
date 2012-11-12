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
using Main.Core.Commands.Synchronization;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Register;

namespace Web.CAPI.Controllers
{
    using System.Linq;

    using Main.Core.View;
    using Main.Core.View.Device;

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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        public DeviceController(IViewRepository repository)
        {
            this.viewRepository = repository;
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
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new RegisterDeviceCommand(data.Description, data.SecretKey, data.TabletId, TODO));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get Secret Key
        /// </summary>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        /// <returns>
        /// Return SecretKey
        /// </returns>
        public JsonResult GetPublicKey(Guid tabletId)
        {
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(tabletId));
            return this.Json(new { PublicKey = model.Items.FirstOrDefault().SecretKey }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}