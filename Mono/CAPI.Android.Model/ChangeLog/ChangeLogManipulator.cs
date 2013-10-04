using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.ChangeLog
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
            fileChangeLogStore = changeLogStore;
        }

        public IList<ChangeLogShortRecord> GetClosedDraftChunksIds()
        {
            return
                draftChangeLog.Filter(c => c.IsClosed)
                              .Select(d => new ChangeLogShortRecord(Guid.Parse(d.Id), Guid.Parse(d.EventSourceId)))
                              .ToList();

        }

        public string GetDraftRecordContent(Guid recordId)
        {
            return fileChangeLogStore.GetChangesetContent(recordId);
        }

        public void CreatePublicRecord(Guid recordId)
        {
            publicChangeLog.Store(new PublicChangeSetDTO(recordId, DateTime.Now),
                                  recordId);
        }

        #endregion


        #region draft

        public void CreateOrReopenDraftRecord(Guid eventSourceId/*, long start*/)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record != null)
            {
                //record.Start = start;
                record.IsClosed = false;
                draftChangeLog.Store(record, Guid.Parse(record.Id));
                return;
            }
            var recordId = Guid.NewGuid();
            draftChangeLog.Store(new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, false), recordId);
        }

        public void CloseDraftRecord(Guid eventSourceId)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record == null)
                return;

            record.IsClosed = true;
            var recordId = Guid.Parse(record.Id);
            var events = BuildEventStreamOfLocalChangesToSend(eventSourceId);
                //BuildEventStreamForSendByEventSourceId(eventSourceId, record.Start);

            fileChangeLogStore.SaveChangeset(events, recordId);
            draftChangeLog.Store(record, recordId);
        }

        /*private AggregateRootEvent[] BuildEventStreamForSendByEventSourceId(Guid eventSourceId, long start)
        {
            var storedEvents = eventStore.ReadFrom(eventSourceId, start, long.MaxValue).ToList();

            /*var indexOfLastCompleteEvent = GetIndexOfLastCompleteEvent(storedEvents);#1#

            var events =
                storedEvents/*.Take(indexOfLastCompleteEvent)#1#.Where(EventIsActive).Select(e => new AggregateRootEvent(e)).ToArray();

            return events;
        }*/


        private AggregateRootEvent[] BuildEventStreamOfLocalChangesToSend(Guid eventSourceId)
        {
            var storedEvents = eventStore.ReadFrom(eventSourceId, 0, long.MaxValue).ToList();

            List<AggregateRootEvent> eventsToSend = new List<AggregateRootEvent>(); 
            
            for (int i = storedEvents.Count - 1; i >= 0; i--)
            {
                if (storedEvents[i].Payload is InterviewSynchronized)
                {
                    break;
                }

                if (EventIsActive(storedEvents[i]))
                    eventsToSend.Add(new AggregateRootEvent(storedEvents[i]));
            }

            eventsToSend.Reverse();
            return eventsToSend.ToArray();
        }


        /*private static int GetIndexOfLastCompleteEvent(List<CommittedEvent> storedEvents)
        {
            int indexOfLastCompleteEvent = storedEvents.Count - 1;
            for (int i = storedEvents.Count - 1; i >= 0; i--)
            {
                if (storedEvents[i].Payload is InterviewCompleted)
                {
                    indexOfLastCompleteEvent = i;
                    break;
                }
            }
            return indexOfLastCompleteEvent;
        }*/

        private bool EventIsActive(CommittedEvent committedEvent)
        {
            var eventType = committedEvent.Payload;

            if (eventType is AnswerDeclaredInvalid)
                return false;

            if (eventType is AnswerDeclaredValid)
                return false;

            if (eventType is GroupDisabled)
                return false;

            if (eventType is GroupEnabled)
                return false;

            if (eventType is QuestionDisabled)
                return false;

            if (eventType is QuestionEnabled)
                return false;
            
            return true;
        }

        /*public void ReopenDraftRecord(Guid eventSourceId)
        {
            var record = GetLastDraftRecord(eventSourceId);
            if (record == null)
                return;
            record.IsClosed = false;

            var recordId = Guid.Parse(record.Id);

            fileChangeLogStore.DeleteDraftChangeSet(recordId);
            draftChangeLog.Store(record, recordId);
        }*/
        
        public void CleanUpChangeLogByRecordId(Guid recordId)
        {
            var record = draftChangeLog.GetById(recordId);
            
            if (record == null)
                return;

            draftChangeLog.Remove(recordId);
            fileChangeLogStore.DeleteDraftChangeSet(recordId);
        }


        public void CleanUpChangeLogByEventSourceId(Guid eventSourceId)
        {
            string eventSource = eventSourceId.ToString();
            var record = draftChangeLog.Filter(c => c.EventSourceId == eventSource).FirstOrDefault();
            if (record == null)
                return;
            CleanUpChangeLogByRecordId(Guid.Parse(record.Id));
        }

        #endregion

        private DraftChangesetDTO GetLastDraftRecord(Guid eventSourceId)
        {
            var evtIdAsString = eventSourceId.ToString();
            var record = draftChangeLog.Filter(c => c.EventSourceId == evtIdAsString).FirstOrDefault();
            return record;
        }

        public string GetPathToBakupFile()
        {
            throw new NotImplementedException();
        }

        public void RestoreFromBakupFolder(string path)
        {
            throw new NotImplementedException();
        }
    }
}