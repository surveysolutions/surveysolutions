using System;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;

namespace WB.UI.Interviewer.ViewModel.Synchronization
{
    public class PublicChangeSetDTO : DenormalizerRow
    {
        public PublicChangeSetDTO(Guid id, DateTime timeStamp)
        {
            this.Id = id.FormatGuid();
            this.TimeStamp = timeStamp.Ticks;
        }

        public PublicChangeSetDTO()
        {
        }

        public long TimeStamp { get; set; }
     

    }
}