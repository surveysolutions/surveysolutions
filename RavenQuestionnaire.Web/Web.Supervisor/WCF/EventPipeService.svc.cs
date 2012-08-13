using System;
using System.ServiceModel.Activation;
using Ninject;
using RavenQuestionnaire.Core.Events;
using SynchronizationMessages.CompleteQuestionnaire;

namespace Web.Supervisor.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class EventPipeService : IEventPipe
    {
        private IKernel kernel;
        public EventPipeService(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public ErrorCodes Process(EventSyncMessage request)
        {
            try
            {
               // kernel.Get<ICommandInvoker>().Execute(request.Command, request.CommandKey, request.SynchronizationKey);
                var eventStore= kernel.Get<IEventSync>();
                eventStore.WriteEvents(request.Command);
                return ErrorCodes.None;
            }
            catch (Exception)
            {

                return ErrorCodes.Fail;
            }
        }
    }
}
