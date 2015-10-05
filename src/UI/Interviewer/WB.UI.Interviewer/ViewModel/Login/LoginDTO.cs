using System;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;

namespace WB.UI.Interviewer.ViewModel.Login
{
    public class LoginDTO : DenormalizerRow
    {
        public LoginDTO(Guid id, string login, string password, bool isLockedBySupervisor, bool isLockedByHQ, Guid supervisorId)
        {
            this.Id = id.FormatGuid();
            this.Login = login;
            this.Password = password;
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
            this.Supervisor = supervisorId.FormatGuid();
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