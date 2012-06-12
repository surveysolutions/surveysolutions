using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Documents;
using SynchronizationMessages.CompleteQuestionnaire;

namespace DataEntryWCFServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EventDocumentSyncService : IEventDocumentSync
    {
        private IKernel kernel;
        public EventDocumentSyncService(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ErrorCodes Process(EventSyncMessage request)
        {
            try
            {
                kernel.Get<ICommandInvoker>().Execute(request.Command, request.CommandKey, request.SynchronizationKey);

                return ErrorCodes.None;
            }
            catch (Exception)
            {

                return ErrorCodes.Fail;
            }
        }
    }
}
