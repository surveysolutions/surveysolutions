using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;

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