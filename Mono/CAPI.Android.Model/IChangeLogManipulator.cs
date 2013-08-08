using System;
using System.Collections.Generic;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Core.Model
{
    public interface IChangeLogManipulator
    {
        void OpenDraftRecord(Guid eventSourceId, long start);
        void CloseDraftRecord(Guid eventSourceId, long end);
        void ReopenDraftRecord(Guid eventSourceId);

        IList<ChangeLogShortRecord> GetClosedDraftChunksIds();
        string GetDraftRecordContent(Guid recordId);

        void CreatePublicRecord(Guid recordId);
        void CleanUpChangeLogByRecordId(Guid recordId);

        void CleanUpChangeLogByEventSourceId(Guid eventSourceId);
    }

    public class ChangeLogShortRecord
    {
        public ChangeLogShortRecord(Guid recordId, Guid eventSourceId)
        {
            RecordId = recordId;
            EventSourceId = eventSourceId;
        }

        public Guid RecordId { get; private set; }
        public Guid EventSourceId { get; private set; }
    }
}