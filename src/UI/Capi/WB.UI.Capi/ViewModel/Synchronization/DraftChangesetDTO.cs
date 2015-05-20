using System;

using CAPI.Android.Core.Model.ViewModel.Synchronization;

using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Capi.ViewModel.Synchronization
{
    public class DraftChangesetDTO : PublicChangeSetDTO
    {
        public DraftChangesetDTO(Guid id, Guid eventSourceId, DateTime timeStamp, bool isClosed, string userId)
            : base(id, timeStamp)
        {
            this.EventSourceId = eventSourceId.FormatGuid();
            this.IsClosed = isClosed;
            this.UserId = userId;
        }

        public DraftChangesetDTO()
        {
        }

        public string EventSourceId { get; set; }
        public bool IsClosed { get; set; }
        public string UserId { get; set; }
    }
}