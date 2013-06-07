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
using CAPI.Android.Core;
using Main.Synchronization.Credentials;

namespace CAPI.Android.Syncronization.Handshake
{
    public class RestHandshake
    {
      
        private readonly string baseAddress;
 //       private readonly Context context;

        public RestHandshake(string baseAddress)
        {
        
            this.baseAddress = baseAddress;
        }

        public Guid Execute( string login,string password, string deviceId, string appID, Guid? lastState)
        {
        
            return Guid.NewGuid();
        }
    }
}