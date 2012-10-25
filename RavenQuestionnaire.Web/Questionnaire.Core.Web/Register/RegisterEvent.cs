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
    using System.Web;

    using Main.Core.Commands.Synchronization;
    using Main.Core.Events;
    using Main.Core.Events.Synchronization;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Newtonsoft.Json;

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
        void Register(HttpPostedFileBase uploadFile);

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
        /// Synchronizer field
        /// </summary>
        private readonly IEventSync synchronizer;

        /// <summary>
        /// RSACryptoService field
        /// </summary>
        private readonly IRSACryptoService cryptoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterEvent"/> class.
        /// </summary>
        /// <param name="sync">
        /// The sync.
        /// </param>
        /// <param name="service">
        /// The service.
        /// </param>
        public RegisterEvent(IEventSync sync, IRSACryptoService service)
        {
            this.synchronizer = sync;
            this.cryptoService = service;
        }

        /// <summary>
        /// Create new event with publicKey of device
        /// </summary>
        /// <param name="uploadFile">
        /// The upload file.
        /// </param>
        public void Register(HttpPostedFileBase uploadFile)
        {
            uploadFile.InputStream.Position = 0;
            var data = string.Empty;
            var buffer = new byte[uploadFile.InputStream.Length];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = uploadFile.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                data = Encoding.Default.GetString(ms.ToArray());
            }

            var result = JsonConvert.DeserializeObject<RegisterData>(data);
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new RegisterNewDeviceCommand(result.Description, result.SecretKey, result.TabletId));
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
            var registerEvent = this.synchronizer.ReadEvents().Where(t => (t.Payload as NewDeviceRegistered).TabletId == tabletId).FirstOrDefault();
            var data = new RegisterData { Event = registerEvent, TabletId = tabletId, SecretKey = this.cryptoService.GetPublicKey().Modulus };
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var stream = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            using (var ms = new MemoryStream())
            {
                var arr = new byte[stream.Length];
                ms.Read(arr, 0, stream.Length);
                return ms.ToArray();
            }
        }
    }
}
