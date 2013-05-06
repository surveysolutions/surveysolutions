using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.View.User;
using Main.Synchronization.Credentials;

namespace CAPI.Android.Core.Model.Authorization
{
    public interface IAuthentication
    {
        UserLight CurrentUser { get; }
        SyncCredentials RequestSyncCredentials();
        bool IsLoggedIn { get; }
        bool LogOn(string userName, string password);
        void LogOff();
    }
}