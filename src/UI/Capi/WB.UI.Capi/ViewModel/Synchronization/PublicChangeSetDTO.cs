using System;

using WB.Core.GenericSubdomains.Portable;
using WB.UI.Capi.Implementations.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Synchronization
{
    public class PublicChangeSetDTO : DenormalizerRow
    {
        public PublicChangeSetDTO(Guid id, DateTime timeStamp)
        {
            Id = id.FormatGuid();
            TimeStamp = timeStamp.Ticks;
        }

        public PublicChangeSetDTO()
        {
        }

        public long TimeStamp { get; set; }
     

    }
}