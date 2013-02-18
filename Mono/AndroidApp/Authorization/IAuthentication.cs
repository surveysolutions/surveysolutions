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

namespace AndroidApp.Authorization
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        bool IsLoggedIn { get; }
        bool LogOn(string userName, string password);
        void LogOff();
    }
}