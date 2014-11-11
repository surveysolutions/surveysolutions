using System;
using Ncqrs.Commanding;

namespace Main.Core.Commands.Synchronization
{
    [Serializable]
    public class RegisterNewSupervisorCommand : CommandBase
    {
        public RegisterNewSupervisorCommand(string description, byte[] publicKeyCapi, Guid tabletId)
        {
            this.PublicKeyCapi = publicKeyCapi;
            this.Description = description;
            this.TabletId = tabletId;
            this.RegisteredDate = DateTime.Now;
        }

        public Guid RegisterGuid { get; set; }

        public string Description { get; set; }

        public byte[] PublicKeyCapi { get; set; }

        public DateTime RegisteredDate { get; set; }

        public Guid TabletId { get; set; }
    }
}
