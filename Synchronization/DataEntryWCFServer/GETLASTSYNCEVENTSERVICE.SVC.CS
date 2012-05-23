using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using RavenQuestionnaire.Core;
using SynchronizationMessages.Handshake;

namespace DataEntryWCFServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetLastSyncEventService" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GetLastSyncEventService : IGetLastSyncEvent
    {
        private ICommandInvoker invoker;

        public GetLastSyncEventService(ICommandInvoker invoker)
        {
            this.invoker = invoker;
        }


        public Guid? Process(Guid clientGuid)
        {
            throw new NotImplementedException();
        }
    }
}
