using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WB.Core.SharedKernel.Structures.Synchronization
{
    public class FileSyncDescription
    {
        public string Description { get; set; }

        public string OriginalFile { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}
