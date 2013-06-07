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
using Main.Synchronization.Credentials;

namespace CAPI.Android.Syncronization.Handshake
{
    public class RestHandshake
    {
        private readonly ISyncAuthenticator validator;
        private readonly string baseAddress;

        public RestHandshake(string baseAddress, ISyncAuthenticator validator)
        {
            this.validator = validator;
            this.baseAddress = baseAddress;
        }

        public Guid Execute( /*string login,string password, string deviceId,*/ Guid? lastState)
        {
            var currentCredentials = validator.RequestCredentials();
            return Guid.NewGuid();
        }
    }
}