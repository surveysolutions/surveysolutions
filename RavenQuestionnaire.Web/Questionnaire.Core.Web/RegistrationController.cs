// -----------------------------------------------------------------------
// <copyright file="RegistrationController.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Main.Core.Entities;
    using System.Web.Mvc;
    using Main.Core.View;
    using Main.Core.View.Device;
    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Main.Core.Commands.Synchronization;
    using System.Security.Cryptography;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class RegistrationController : Controller
    {
        private IViewRepository viewRepository;

        public RegistrationController(IViewRepository repository)
        {
            this.viewRepository = repository;
        }

        /// <summary>
        /// save info about register device to database
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// boolean about successful saving
        /// </returns>
        protected bool SaveRegistration(RegisterData data)
        {
            try
            {
                data.PublicKey = this.GetARPublicKey(data);

                var model = this.viewRepository.Load<DeviceItemViewInputModel, DeviceItemView>(new DeviceItemViewInputModel(data.RegistrationId));
                var commandService = NcqrsEnvironment.Get<ICommandService>();

                if (model.RegistrationId != Guid.Empty)
                {
                    commandService.Execute(new UpdateRegisterDeviceCommand(data.Description, data.PublicKey, data.SecretKey, data.Registrator));
                }
                else
                {
                    commandService.Execute(new RegisterDeviceCommand(data.Description, data.PublicKey, data.SecretKey, data.RegistrationId, data.Registrator));
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// info about register device
        /// </summary>
        /// <param name="registrator">
        /// The registrator.
        /// </param>
        /// <returns>
        /// register device view
        /// </returns>
        protected DeviceView GetRegisteredData(Guid registrator)
        {
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(registrator));
            return model;
        }

        #region Helpers

        protected abstract Guid GetARPublicKey(RegisterData data);

        #endregion
    }
}
