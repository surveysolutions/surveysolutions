using System;

using WB.Core.GenericSubdomains.Portable;
using WB.UI.Capi.Implementations.DenormalizerStorage;

namespace CAPI.Android.Core.Model.ViewModel.Login
{
    public class LoginDTO : DenormalizerRow
    {
        public LoginDTO(Guid id, string login, string password, bool isLockedBySupervisor, bool isLockedByHQ, Guid supervisorId)
        {
            Id = id.FormatGuid();
            Login = login;
            Password = password;
            IsLockedBySupervisor = isLockedBySupervisor;
            IsLockedByHQ = isLockedByHQ;
            Supervisor = supervisorId.FormatGuid();
        }

        public LoginDTO()
        {
        }

        public string Login { get; set; }
        public string Password { get;  set; }
        public bool IsLockedBySupervisor { get; set; }
        public bool IsLockedByHQ { get; set; }

        public string Supervisor { get; set; }
    }
}