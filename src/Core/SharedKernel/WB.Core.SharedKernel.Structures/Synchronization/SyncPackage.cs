using System;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class SyncPackage
    {
        public Guid Id { get; set; }
        public Guid SyncProcessKey { get; set; }
        public SyncItem SyncItem { get; set; }
    }
}
