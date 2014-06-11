using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContext.Capi.Synchronization.Synchronization.ChangeLog
{
    public interface IChangeLogManipulator
    {
        void CreateOrReopenDraftRecord(Guid eventSourceId);
        void CloseDraftRecord(Guid eventSourceId);
        
        IList<ChangeLogShortRecord> GetClosedDraftChunksIds();
        string GetDraftRecordContent(Guid recordId);

        void CreatePublicRecord(Guid recordId);
        void CleanUpChangeLogByRecordId(Guid recordId);

        void CleanUpChangeLogByEventSourceId(Guid eventSourceId);
    }
}