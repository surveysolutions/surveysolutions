using System;
using SQLite.Net.Attributes;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.Interviewer.ViewModel.Login
{
    [Obsolete]
    public class LoginDTO : IView
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Login { get; set; }
        public string Password { get;  set; }
        public bool IsLockedBySupervisor { get; set; }
        public bool IsLockedByHQ { get; set; }

        public string Supervisor { get; set; }
    }
}