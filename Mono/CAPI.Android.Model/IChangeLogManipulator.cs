using System;
using System.Collections.Generic;

namespace CAPI.Android.Core.Model
{
    public interface IChangeLogManipulator
    {
        void OpenDraftRecord(Guid eventSourceId, long start);
        void CloseDraftRecord(Guid eventSourceId, long end);
        void ReopenDraftRecord(Guid eventSourceId);

        IDictionary<Guid, Guid> GetClosedDraftChunksIds();
        string GetDraftRecordContent(Guid recordId);

        void CreatePublicRecord(Guid recordId);
        Guid MarkDraftChangesetAsPublicAndReturnARId(Guid recordId);
    }
}