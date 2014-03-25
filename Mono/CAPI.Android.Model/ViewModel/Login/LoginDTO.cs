using System;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using WB.Core.GenericSubdomains.Utils;

namespace CAPI.Android.Core.Model.ViewModel.Login
{
    public class LoginDTO : DenormalizerRow
    {
        public LoginDTO(Guid id, string login, string password, bool isLocked, Guid supervisorId)
        {
            Id = id.FormatGuid();
            Login = login;
            Password = password;
            IsLocked = isLocked;
            Supervisor = supervisorId.FormatGuid();
        }

        public LoginDTO()
        {
        }

        public string Login { get; set; }
        public string Password { get;  set; }
        public bool IsLocked { get; set; }

        public string Supervisor { get; set; }
    }
}