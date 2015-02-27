using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Capi.ChangeLog
{
    public interface IChangeLogManipulator
    {
        void CreateOrReopenDraftRecord(Guid eventSourceId, Guid userId);
        void CloseDraftRecord(Guid eventSourceId, Guid userId);
        
        IList<ChangeLogShortRecord> GetClosedDraftChunksIds(Guid userId);
        string GetDraftRecordContent(Guid recordId);

        void CreatePublicRecord(Guid recordId);
        void CleanUpChangeLogByRecordId(Guid recordId);

        void CleanUpChangeLogByEventSourceId(Guid eventSourceId);
    }
}