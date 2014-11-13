using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Synchronization.Documents
{
    public class SyncDeviceRegisterDocument : IView
    {
        public Guid PublicKey { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime ModificationDate { get; set; }

        public Guid TabletId { get; set; }

        public byte[] SecretKey { get; set; }

        public string Description { get; set; }

        public Guid Registrator { get; set; }
    }
}
