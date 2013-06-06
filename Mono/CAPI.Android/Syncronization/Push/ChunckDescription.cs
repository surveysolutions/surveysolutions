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

namespace CAPI.Android.Syncronization.Push
{
    public class ChunckDescription
    {
        public ChunckDescription(Guid id, byte[] content)
        {
            Id = id;
            Content = content;
        }

        public Guid Id { get; private set; }
        public byte[] Content { get; private set; }
    }
}