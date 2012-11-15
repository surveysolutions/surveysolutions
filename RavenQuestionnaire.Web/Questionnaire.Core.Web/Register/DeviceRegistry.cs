// -----------------------------------------------------------------------
// <copyright file="DeviceRegistry.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Register
{
    using System;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Entities;
    using Main.Core.View;
    using Main.Core.View.Device;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    /// <summary>
    /// register interface
    /// </summary>
    public interface IDeviceRegistry
    {
        /// <summary>
        /// info about register device
        /// </summary>
        /// <param name="register">
        /// The register.
        /// </param>
        /// <returns>
        ///  register device view
        /// </returns>
        DeviceView GetRegisterData(Guid register);

        /// <summary>
        /// save info about register device to database
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// boolean about successful saving
        /// </returns>
        bool SaveRegistrator(RegisterData data);
    }

    /// <summary>
    /// Service for device registration
    /// </summary>
    public class DeviceRegistry : IDeviceRegistry
    {
        /// <summary>
        /// Field of ViewRepository
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceRegistry"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        public DeviceRegistry(IViewRepository repository)
        {
            this.viewRepository = repository;
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
        public DeviceView GetRegisterData(Guid registrator)
        {
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(registrator));
            return model;
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
        public bool SaveRegistrator(RegisterData data)
        {
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new RegisterDeviceCommand(data.Description, data.SecretKey, data.RegistrationId, data.Registrator));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
