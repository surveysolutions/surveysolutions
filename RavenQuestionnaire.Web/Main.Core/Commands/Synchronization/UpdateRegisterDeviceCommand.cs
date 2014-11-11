using System;
using Ncqrs.Commanding;

namespace Main.Core.Commands.Synchronization
{
    [Serializable]
    public class UpdateRegisterDeviceCommand : CommandBase
    {
        public UpdateRegisterDeviceCommand(string description, Guid publicKey, byte[] secretKey, Guid registrator)
        {
            this.PublicKey = publicKey;
            this.Registrator = registrator;
            this.Description = description;
            this.SecretKey = secretKey;
        }

        public Guid PublicKey { get; set; }

        public Guid Registrator { get; set; }

        public string Description { get; set; }

        public byte[] SecretKey { get; set; }
    }
}
