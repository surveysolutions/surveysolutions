using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization
{
    public class SyncPackage
    {
        public Guid Id;

        public List<SyncItem> ItemsContainer;

        public bool Status;

        public string Message;

        public SyncPackage()
        {
            ItemsContainer = new List<SyncItem>();
            Id = Guid.NewGuid();
        }
    }
}
