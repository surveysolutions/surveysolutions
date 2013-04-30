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
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Login
{
    public class LoginDTO : DenormalizerRow
    {
        public LoginDTO(Guid id, string login, string password, bool isLocked)
        {
            Id = id.ToString();
            Login = login;
            Password = password;
            IsLocked = isLocked;
        }

        public LoginDTO()
        {
        }

        public string Login { get; set; }
        public string Password { get;  set; }
        public bool IsLocked { get; set; }
    }
}