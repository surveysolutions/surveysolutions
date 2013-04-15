using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class SyncItemsMetaContainer
    {
        public SyncItemsMetaContainer()
        {
            ARId  = new List<Tuple<string, Guid>>();
        }

        public List<Tuple<string,Guid>> ARId { set; get; }
    }
}
