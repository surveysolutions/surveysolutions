using System.Linq;
using Android.Content;
using Android.Net;

namespace WB.Core.GenericSubdomain.Rest.Android
{
    internal class AndroidNetworkService : INetworkService
    {
        private readonly Context context;

        public AndroidNetworkService(Context context)
        {
            this.context = context;
        }

        public bool IsNetworkEnabled()
        {
            var cm = (ConnectivityManager)this.context.GetSystemService(Context.ConnectivityService);

            return cm.GetAllNetworkInfo().Where(networkInfo => networkInfo.Type == ConnectivityType.Wifi || networkInfo.Type == ConnectivityType.Mobile)
                     .Any(networkInfo => networkInfo.IsConnectedOrConnecting);
        }
    }
}