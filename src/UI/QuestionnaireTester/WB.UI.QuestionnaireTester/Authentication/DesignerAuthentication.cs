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

        public bool LogOn(string userName, string password, CancellationToken cancellationToken)
        {
            if (CapiTesterApplication.DesignerServices.Login(userName, password, cancellationToken))
            {
                CurrentUser = new UserInfo(userName, password);
                return true;
            }
            return false;
        }

        public void LogOff()
        {
            CurrentUser = null;
        }
    }
}