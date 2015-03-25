using System.Linq;
using Android.Content;
using Android.Net;
using Cirrious.CrossCore.Droid.Platform;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class AndroidNetworkService : INetworkService
    {
        private IMvxAndroidCurrentTopActivity context
        {
            get {  return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }

        public bool IsNetworkEnabled()
        {
            var cm = (ConnectivityManager)this.context.Activity.GetSystemService(Context.ConnectivityService);

            return cm.GetAllNetworkInfo().Where(networkInfo => networkInfo.Type == ConnectivityType.Wifi || networkInfo.Type == ConnectivityType.Mobile)
                     .Any(networkInfo => networkInfo.IsConnectedOrConnecting);
        }
    }
}