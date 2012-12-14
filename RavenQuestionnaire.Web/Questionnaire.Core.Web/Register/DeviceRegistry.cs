// -----------------------------------------------------------------------
// <copyright file="DeviceRegistry.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Register
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Discovery;
    using System.Data;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Entities;
    using Main.Core.View;
    using Main.Core.View.Device;
    using Main.Core.WCF;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;


    /// <summary>
    /// register interface
    /// </summary>
    public interface IDeviceRegistry
    {
        /// <summary>
        /// info about registered device or supervisor
        /// </summary>
        /// <param name="registerarId">
        /// The registrar who made this registration
        /// </param>
        /// <returns>
        ///  register device view
        /// </returns>
        DeviceView GetRegisteredData(Guid registerarId);

        /// <summary>
        /// Save info about registered device or supervisor to database
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// boolean about successful saving
        /// </returns>
        bool SaveRegistration(RegisterData data);

        /// <summary>
        /// Ask remote registration via WCF to authorize registration request
        /// </summary>
        /// <param name="registrarUrl">WCF path</param>
        /// <param name="data">Data to register</param>
        /// <returns>Remote registrator access status</returns>
        bool AuthorizeByRemoteRegistrator(string registrarUrl, RegisterData data);

        /// <summary>
        /// Ask remoter registrator for registered data for registration id
        /// </summary>
        /// <param name="registrarUrl"></param>
        /// <param name="registrationId"></param>
        /// <returns></returns>
        List<RegisterData> CheckRemoteAuthorization(string registrarUrl, Guid registrationId);
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

        private IAuthorizationService channelService = null;
        private string registrarUrl = string.Empty;

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

        #region Helpers

        /// <summary>
        /// Lookup supervisor's service by known url
        /// </summary>
        /// <param name="registrarUrl"></param>
        /// <returns></returns>
        private IAuthorizationService LookupSupervisor(string registrarUrl)
        {
            if (this.channelService != null && string.Compare(registrarUrl, this.registrarUrl, true) == 0)
            {
                return this.channelService;
            }

            this.registrarUrl = string.Empty;

            try
            {
                var address = new EndpointAddress(registrarUrl);
                var endpoints = MetadataResolver.Resolve(typeof(IAuthorizationService), address);

                if (endpoints.Count < 1)
                    return null;

                var factory = new ChannelFactory<IAuthorizationService>(endpoints[0].Binding, endpoints[0].Address);

                this.channelService = factory.CreateChannel();
                this.registrarUrl = registrarUrl;

                return this.channelService;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// info about register device
        /// </summary>
        /// <param name="registrator">
        /// The registrator.
        /// </param>
        /// <returns>
        /// register device view
        /// </returns>
        public DeviceView GetRegisteredData(Guid registrator)
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
        public bool SaveRegistration(RegisterData data)
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

        public bool AuthorizeByRemoteRegistrator(string registrarUrl, RegisterData data)
        {
            var regService = LookupSupervisor(registrarUrl);
            if (regService == null)
                return false;

            try
            {
                return regService.AuthorizeDevice(new AuthorizationPacket() { Data = data });
            }
            catch
            {
                return false;
            }
        }

        public List<RegisterData> CheckRemoteAuthorization(string registrarUrl, Guid registrationId)
        {
            var regService = LookupSupervisor(registrarUrl);
            if (regService == null)
                return null;

            try
            {
                var packets = regService.GetAuthorizationPackets().Packets.Where(
                    p => p.Data.RegistrationId == registrationId && p.IsAuthorized);

                if(packets.FirstOrDefault() == null)
                    return null;

                var res = new List<RegisterData>();
                foreach (var p in packets)
                    res.Add(p.Data);

                return res;
            }
            catch
            {
                return null;
            }
        }
    }
}
