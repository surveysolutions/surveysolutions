using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Syncronization.Push
{
    public class RestPush
    {
        private const int MillisecondsTimeout=1000;
        private readonly string baseAddress;

        public RestPush(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        public void PushChunck(Guid chunckId, byte[] content, Guid synckId)
        {
            Thread.Sleep(MillisecondsTimeout);
        }
    }
}