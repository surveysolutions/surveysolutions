using System;

namespace CAPI.Android.Core.Model.ViewModel.Synchronization
{
    public class DraftChangesetDTO : PublicChangeSetDTO
    {
        public DraftChangesetDTO(Guid id, Guid eventSourceId, DateTime timeStamp, /*long start,*/ bool isClosed)
            : base(id, timeStamp)
        {
            EventSourceId = eventSourceId.ToString();
            //Start = start;
            IsClosed = isClosed;
        }

        public DraftChangesetDTO()
        {
        }
        public string EventSourceId { get; set; }
        //public long Start { get; set; }
        public bool IsClosed { get; set; }
    }
}