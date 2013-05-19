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

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class StoredEventWithoutPayload
    {
       
        public string EventSourceId { get; set; }
        
        public string EventId { get; set; }
        public long Sequence { get; set; }
    }
}