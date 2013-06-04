using System;
using Main.Core.Events;

namespace CAPI.Android.Core.Model.ChangeLog
{
    public interface IChangeLogManipulator
    {
        void OpenDraftRecord(Guid eventSourceId, long start);
        void CloseDraftRecord(Guid eventSourceId, long end);
        void ReopenDraftRecord(Guid eventSourceId);
        void CreatePublicRecord(Guid recordId, Guid eventSourceId);
        void MarkDraftChangesetAsPublic(Guid recordId);
    }
}