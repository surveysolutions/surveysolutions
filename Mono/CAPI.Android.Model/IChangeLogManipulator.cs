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

        IDictionary<Guid, Guid> GetClosedDraftChunksIds();
        SyncItem GetDraftRecordContent(Guid recordId);

        void CreatePublicRecord(Guid recordId);
        void CleanUpChangeLogByRecordId(Guid recordId);

        void CleanUpChangeLogByEventSourceId(Guid eventSourceId);
    }
}