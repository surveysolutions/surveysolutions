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

namespace CAPI.Android.Syncronization.Handshake
{
    public class RestHandshake
    {
        public IDictionary<Guid, bool> GetChuncks()
        {
            Thread.Sleep(1000);
            var retval = new Dictionary<Guid, bool>();
            for (int i = 0; i < 3; i++)
            {
                retval.Add(Guid.NewGuid(), false);
            }

            return retval;
        }
    }
}