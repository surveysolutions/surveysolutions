using System;
using System.ServiceModel.Activation;
using Ninject;
using RavenQuestionnaire.Core;
using SynchronizationMessages.CompleteQuestionnaire;

namespace Web.CAPI.WCF
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
