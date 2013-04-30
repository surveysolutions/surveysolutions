using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Main.Core.Events;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class SyncItemsMetaContainer
    {
        public SyncItemsMetaContainer()
        {
            ARId = new List<SyncItemsMeta>();
        }

        public List<SyncItemsMeta> ARId { set; get; }
    }

}