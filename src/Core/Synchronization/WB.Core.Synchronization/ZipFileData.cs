using System;
using System.Collections.Generic;
using Main.Core.Events;

namespace WB.Core.Synchronization
{
    public class ZipFileData
    {
        public ZipFileData()
        {
            this.ImportDate = DateTime.Now;
            this.CreationDate = DateTime.UtcNow;
        }

        protected DateTime CreationDate { get; set; }
        public Guid ClientGuid { get; set; }
        public IEnumerable<AggregateRootEvent> Events { get; set; }
        public DateTime ImportDate { get; set; }
    }
}