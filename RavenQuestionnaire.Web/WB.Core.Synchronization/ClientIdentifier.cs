using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.Synchronization
{
    public class ClientIdentifier
    {
        public Guid ClientInstanceKey;
        public string ClientDeviceKey;
        public string ClientVersionIdentifier;
        
        public Guid? LastSyncIdentifier;

        public Guid? CurrentProcessKey;

        public ClientIdentifier()
        {

        }
    }
}
