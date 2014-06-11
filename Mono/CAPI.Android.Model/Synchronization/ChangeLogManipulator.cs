using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Events;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContext.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace CAPI.Android.Core.Model.Synchronization
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

        public IList<ChangeLogShortRecord> GetClosedDraftChunksIds()
        {
            return
                this.draftChangeLog.Filter(c => c.IsClosed)
                              .Select(d => new ChangeLogShortRecord(Guid.Parse(d.Id), Guid.Parse(d.EventSourceId)))
                              .ToList();

        }

        public string GetDraftRecordContent(Guid recordId)
        {
            return this.fileChangeLogStore.GetChangesetContent(recordId);
        }

        public void CreatePublicRecord(Guid recordId)
        {
            this.publicChangeLog.Store(new PublicChangeSetDTO(recordId, DateTime.Now),
                                  recordId);
        }

        #endregion


        #region draft

        public void CreateOrReopenDraftRecord(Guid eventSourceId)
        {
            var record = this.GetLastDraftRecord(eventSourceId);
            if (record != null)
            {
                record.IsClosed = false;
                this.draftChangeLog.Store(record, Guid.Parse(record.Id));
                return;
            }
            var recordId = Guid.NewGuid();
            this.draftChangeLog.Store(new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, false), recordId);
        }

        public void CloseDraftRecord(Guid eventSourceId)
        {
            var record = this.GetLastDraftRecord(eventSourceId);
            Guid recordId;

            if (record == null)
            {
                recordId = Guid.NewGuid();
                record = new DraftChangesetDTO(recordId, eventSourceId, DateTime.Now, true);
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
            var storedEvents = this.eventStore.ReadFrom(eventSourceId, 0, long.MaxValue).ToList();

            List<AggregateRootEvent> eventsToSend = new List<AggregateRootEvent>(); 
            
            for (int i = storedEvents.Count - 1; i >= 0; i--)
            {
                if (storedEvents[i].Payload is InterviewSynchronized || storedEvents[i].Payload is InterviewOnClientCreated)
                {
                    break;
                }

                if (this.EventIsActive(storedEvents[i]))
                    eventsToSend.Add(new AggregateRootEvent(storedEvents[i]));
            }

            eventsToSend.Reverse();
            return eventsToSend.ToArray();
        }

        private bool EventIsActive(CommittedEvent committedEvent)
        {
            var eventType = committedEvent.Payload;

            if (eventType is AnswerDeclaredInvalid)
                return false;

            if (eventType is AnswerDeclaredValid)
                return false;

            if (eventType is AnswersDeclaredInvalid)
                return false;

            if (eventType is AnswersDeclaredValid)
                return false;

            if (eventType is GroupDisabled)
                return false;

            if (eventType is GroupEnabled)
                return false;

            if (eventType is GroupsDisabled)
                return false;

            if (eventType is GroupsEnabled)
                return false;

            if (eventType is QuestionDisabled)
                return false;

            if (eventType is QuestionEnabled)
                return false;

            if (eventType is QuestionsDisabled)
                return false;

            if (eventType is QuestionsEnabled)
                return false;
            
            return true;
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