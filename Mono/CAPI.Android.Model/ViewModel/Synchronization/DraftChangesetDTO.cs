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

namespace CAPI.Android.Core.Model.ViewModel.Synchronization
{
    public class DraftChangesetDTO : PublicChangeSetDTO
    {
        public DraftChangesetDTO(Guid id, Guid eventSourceId, DateTime timeStamp, long start, long? end)
            : base(id, timeStamp)
        {
            EventSourceId = eventSourceId.ToString();
            Start = start;
            End = end;
        }

        public DraftChangesetDTO()
        {
        }
        public string EventSourceId { get; set; }
        public long Start { get; set; }
        public long? End { get; set; }
    }
}