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
        /// </summary>
        /// <param name="register">
        /// The register.
        /// </param>
        /// <returns>
        /// </returns>
        DeviceView GetRegisterData(Guid register);

        bool SaveRegistrator(RegisterData data);
    }

    public class DeviceRegistry : IDeviceRegistry
    {

        private readonly IViewRepository viewRepository;

        public DeviceRegistry(IViewRepository repository)
        {
            this.viewRepository = repository;
        }

        public DeviceView GetRegisterData(Guid registrator)
        {
            var model = this.viewRepository.Load<DeviceViewInputModel, DeviceView>(new DeviceViewInputModel(registrator));
            return model;
        }

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
