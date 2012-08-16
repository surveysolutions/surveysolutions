using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Events;
using SynchronizationMessages.CompleteQuestionnaire;

namespace Web.Supervisor.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "GetAggragateRootListService" in code, svc and config file together.
    public class GetAggragateRootListService : IGetAggragateRootList
    {
        private readonly IEventSync eventStore;

        public GetAggragateRootListService(IEventSync eventStore)
        {
            this.eventStore = eventStore;
        }

        #region Implementation of IGetAggragateRootList

        public ListOfAggregateRootsForImportMessage Process()
        {
            IEnumerable<IEnumerable<AggregateRootEvent>> events = eventStore.ReadEventsByChunks();

            return new ListOfAggregateRootsForImportMessage
                       {Roots = events.Select(e => new ProcessedEventChunk(e)).ToList()};
        }

        #endregion
    }
}