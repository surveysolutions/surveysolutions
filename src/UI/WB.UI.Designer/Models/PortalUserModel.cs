using System;

namespace WB.UI.Designer.Models
{
    public class PortalUserModel
    {
        public PortalUserModel(Guid id, string[] roles, string? login, string? email, string? fullName = "")
        {
            Id = id;
            Roles = roles;
            Login = login;
            Email = email;
            FullName = fullName;
        }

        public Guid Id { get; set; }
        public string[] Roles { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}
