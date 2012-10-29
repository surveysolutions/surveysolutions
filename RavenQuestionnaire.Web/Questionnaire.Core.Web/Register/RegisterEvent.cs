// -----------------------------------------------------------------------
// <copyright file="RegisterEvent.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Register
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Events;
    using Main.Core.Events.Synchronization;
    using Main.Core.View;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing;

    using Newtonsoft.Json;

    using global::Core.Supervisor.Views.Register;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRegisterEvent
    {
        /// <summary>
        /// Register method
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        void Register(byte[] uploadFile);

        /// <summary>
        /// Complete register
        /// </summary>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        /// <returns>
        /// array of byte with event registration
        /// </returns>
        byte[] CompleteRegister(Guid tabletId);
    }

    /// <summary>
    /// RegisterEvent for new device
    /// </summary>
    public class RegisterEvent : IRegisterEvent
    {
        /// <summary>
        /// ViewRepository field
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// RSACryptoService field
        /// </summary>
        private readonly IRSACryptoService cryptoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterEvent"/> class.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public RegisterEvent(IViewRepository repository, IRSACryptoService service)
        {
            this.viewRepository = repository;
            this.cryptoService = service;
        }

        /// <summary>
        /// Create new event with publicKey of device
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        public void Register(byte[] uploadFile)
        {
            var data = Encoding.Default.GetString(uploadFile);
            var result = JsonConvert.DeserializeObject<RegisterData>(data);
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new RegisterNewDeviceCapiCommand(result.Description, result.SecretKey, result.TabletId));
        }

        /// <summary>
        /// Process complete register for device
        /// </summary>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        /// <returns>
        /// returns events 
        /// </returns>
        public byte[] CompleteRegister(Guid tabletId)
        {
            var registerEvent =
                this.viewRepository.Load<RegisterInputModel, RegisterView>(
                    new RegisterInputModel() { TabletId = tabletId });
            var data = new RegisterData
                { Event = registerEvent, TabletId = tabletId, SecretKey = this.cryptoService.GetPublicKey().Modulus };
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var stream = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write(stream);
                }

                return ms.ToArray();
            }
        }
    }
}
