using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Events;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.UI.Capi.ViewModel.Synchronization;

namespace WB.UI.Capi.Syncronization
{
    public class ChangeLogManipulator : IChangeLogManipulator, IBackupable
    {
        private readonly IReadSideRepositoryWriter<PublicChangeSetDTO> publicChangeLog;
        private readonly IFilterableReadSideRepositoryWriter<DraftChangesetDTO> draftChangeLog;
        private readonly IEventStore eventStore;
        private readonly IChangeLogStore fileChangeLogStore;

        #region public

        public ChangeLogManipulator(IReadSideRepositoryWriter<PublicChangeSetDTO> publicChangeLog,
                                    IFilterableReadSideRepositoryWriter<DraftChangesetDTO> draftChangeLog,
                                    IEventStore eventStore, IChangeLogStore changeLogStore)
        {
            this.publicChangeLog = publicChangeLog;
            this.draftChangeLog = draftChangeLog;
            this.eventStore = eventStore;
            this.fileChangeLogStore = changeLogStore;
        }

        public IList<ChangeLogShortRecord> GetClosedDraftChunksIds(Guid userId)
        {
            var userIdAsString = userId.FormatGuid();
            return this.draftChangeLog.Filter(c => c.IsClosed && c.UserId == userIdAsString)
                              .Select(d => new ChangeLogShortRecord(Guid.Parse(d.Id), Guid.Parse(d.EventSourceId)))
                              .ToList();

        }

        public string GetDraftRecordContent(Guid recordId)
        {
            return this.fileChangeLogStore.GetChangesetContent(recordId);
        }

        public void CreatePublicRecord(Guid recordId)
        {
            this.publicChangeLog.Store(new PublicChangeSetDTO(recordId, DateTime.Now), recordId);
        }

        #endregion


        #region draft

        public void CreateOrReopenDraftRecord(Guid eventSourceId, Guid userId)
        {
            var record = this.GetLastDraftRecord(eventSourceId);
            if (record != null)
            {
                record.IsClosed = false;
                this.draftChangeLog.Store(record, Guid.Parse(record.Id));
                return;
            }
            var recordId = Guid.NewGuid();
            this.draftChangeLog.Store(new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, false, userId.FormatGuid()), recordId);
        }

        public void CloseDraftRecord(Guid eventSourceId, Guid userId)
        {
            var record = this.GetLastDraftRecord(eventSourceId);
            Guid recordId;

            if (record == null)
            {
                recordId = Guid.NewGuid();
                record = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, true, userId.FormatGuid());
            }
            else
            {
                record.IsClosed = true;
                recordId = Guid.Parse(record.Id);
            }
            
            var events = this.BuildEventStreamOfLocalChangesToSend(eventSourceId);

            this.fileChangeLogStore.SaveChangeset(events, recordId);
            this.draftChangeLog.Store(record, recordId);
        }

        
        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid eventSourceId)
        {
            var storedEvents = this.eventStore.ReadFrom(eventSourceId, 0, int.MaxValue).ToList();

            List<AggregateRootEvent> eventsToSend = new List<AggregateRootEvent>(); 
            
            for (int i = storedEvents.Count - 1; i >= 0; i--)
            {
                if (storedEvents[i].Payload is InterviewSynchronized || storedEvents[i].Payload is InterviewOnClientCreated)
                {
                    break;
                }

                eventsToSend.Add(new AggregateRootEvent(storedEvents[i]));
            }

            eventsToSend.Reverse();
            return eventsToSend.ToArray();
        }

        public void CleanUpChangeLogByRecordId(Guid recordId)
        {
            var record = this.draftChangeLog.GetById(recordId);
            
            if (record == null)
                return;

            this.draftChangeLog.Remove(recordId);
            this.fileChangeLogStore.DeleteDraftChangeSet(recordId);
        }


        public void CleanUpChangeLogByEventSourceId(Guid eventSourceId)
        {
            string eventSource = eventSourceId.FormatGuid();
            var record = this.draftChangeLog.Filter(c => c.EventSourceId == eventSource).FirstOrDefault();
            if (record == null)
                return;
            this.CleanUpChangeLogByRecordId(Guid.Parse(record.Id));
        }

        #endregion

        private DraftChangesetDTO GetLastDraftRecord(Guid eventSourceId)
        {
            var evtIdAsString = eventSourceId.FormatGuid();
            var record = this.draftChangeLog.Filter(c => c.EventSourceId == evtIdAsString).FirstOrDefault();
            return record;
        }

        public string GetPathToBackupFile()
        {
            throw new NotImplementedException();
        }

        public void RestoreFromBackupFolder(string path)
        {
            throw new NotImplementedException();
        }
    }
}