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
using Main.Core.Entities.SubEntities;
using RestSharp;
using WB.Core.BoundedContexts.Capi;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.QuestionnaireTester.Authentication
{
    public class DesignerAuthentication
    {
        public UserInfo CurrentUser { get; private set; }

        public bool IsLoggedIn
        {
            get { return CurrentUser != null; }
        }

        public bool LogOn(string userName, string password)
        {
            var webExecutor = new AndroidRestUrils("https://192.168.173.1/designer");
            try
            {
                webExecutor.ExcecuteRestRequest<bool>(
                    "Api/Tester/Authorize",
                    new HttpBasicAuthenticator(userName, password), "POST");
                CurrentUser = new UserInfo(userName, password);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void LogOff()
        {
            CurrentUser = null;
        }
    }
}