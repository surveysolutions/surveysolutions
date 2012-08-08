using System;
using Ninject;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Views.Event;
using SynchronizationMessages.Handshake;

namespace RavenQuestionnaire.Web.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetLastSyncEventService" in code, svc and config file together.
    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GetLastSyncEventService : IGetLastSyncEvent
    {
        //ninject wcf bug https://groups.google.com/forum/#!msg/ninject/Q8uGKIjBnyM/U3iM0EWp4_EJ

        private IViewRepository viewRepository;

        public GetLastSyncEventService(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        public Guid? Process(Guid clientGuid)
        {
            var eventItem =
                viewRepository.Load<EventViewInputModel, EventView>(new EventViewInputModel(clientGuid));
            if (eventItem == null)
                return null;
            return eventItem.PublicKey;
        }
    }
}
