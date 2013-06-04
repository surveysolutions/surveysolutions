using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Synchronization
{
    public class PublicChangeSetDTO : DenormalizerRow
    {
        public PublicChangeSetDTO(Guid id, Guid eventSourceId, DateTime timeStamp)
        {
            Id = id.ToString();
            this.EventSourceId = eventSourceId.ToString();
            TimeStamp = timeStamp.Ticks;
        }

        public PublicChangeSetDTO()
        {
        }

        public long TimeStamp { get; set; }
        public string EventSourceId { get; set; }

    }
}