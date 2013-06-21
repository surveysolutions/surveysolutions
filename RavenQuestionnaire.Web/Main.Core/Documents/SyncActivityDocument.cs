using System;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    public class SyncActivityDocument : IView
    {
        public Guid Id { set; get; }

        public Guid DeviceId { set; get; }

        public DateTime CreationDate { set; get; }

        public DateTime LastChangeDate { set; get; }
        
    }
}
