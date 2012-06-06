using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Views.Synchronization
{
    public class SyncProgressInputModel
    {
        public Guid ProcessKey { get; private set; }
        public  SyncProgressInputModel(Guid progressKey)
        {
            this.ProcessKey = progressKey;
        }
    }
}
