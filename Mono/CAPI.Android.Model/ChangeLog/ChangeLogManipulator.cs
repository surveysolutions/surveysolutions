using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Events;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public class ChangeLogManipulator : IChangeLogManipulator
    {
        private readonly IFilterableDenormalizerStorage<PublicChangeSetDTO> publicChangeLog;
        private readonly IFilterableDenormalizerStorage<DraftChangesetDTO> draftChangeLog;
        private readonly IEventStore eventStore;
        private readonly IChangeLogStore fileChangeLogStore;

        #region public

        public ChangeLogManipulator(IFilterableDenormalizerStorage<PublicChangeSetDTO> publicChangeLog, IFilterableDenormalizerStorage<DraftChangesetDTO> draftChangeLog, IEventStore eventStore,IChangeLogStore changeLogStore)
        {
            this.publicChangeLog = publicChangeLog;
            this.draftChangeLog = draftChangeLog;
            this.eventStore = eventStore;
            fileChangeLogStore = changeLogStore;
        }

        public void CreatePublicRecord(Guid recordId, Guid eventSourceId)
        {
            publicChangeLog.Store(new PublicChangeSetDTO(recordId, eventSourceId, DateTime.Now),
                            recordId);
        }

        #endregion


        #region draft

        public void OpenDraftRecord(Guid eventSourceId, long start)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record != null)
                throw new InvalidOperationException("draft record already had been created");
            var recordId = Guid.NewGuid();
            draftChangeLog.Store(new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, start, null), recordId);
        }

        public void CloseDraftRecord(Guid eventSourceId, long end)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record == null)
                return;
            if(record.Start>=end)
                throw new ArgumentException("end is more than start");
            record.End = end;
            var storedEvents = eventStore.ReadFrom(eventSourceId, record.Start, end);
            var events =
                storedEvents.Select(e => new AggregateRootEvent(e)).ToArray();

            fileChangeLogStore.SaveChangeset(events, Guid.Parse(record.Id));
            draftChangeLog.Store(record, Guid.Parse(record.Id));
        }

        public void ReopenDraftRecord(Guid eventSourceId)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record == null)
                return;
            record.End = null;
            var recodId = Guid.Parse(record.Id);
            fileChangeLogStore.DeleteDraftChangeSet(recodId);
            draftChangeLog.Store(record, recodId);
        }

        public void MarkDraftChangesetAsPublic(Guid recordId)
        {
            var record = draftChangeLog.GetById(recordId);
            if (record==null)
                return;
            draftChangeLog.Remove(recordId);
            fileChangeLogStore.DeleteDraftChangeSet(recordId);
            CreatePublicRecord(recordId, Guid.Parse(record.EventSourceId));
        }

        #endregion


        

       
        private DraftChangesetDTO GetLastDraftRecord(Guid eventSourceId)
        {
            var evtIdAsString = eventSourceId.ToString();
            var record = draftChangeLog.Query(c => c.EventSourceId == evtIdAsString).FirstOrDefault();
            return record;
        }

    }
}