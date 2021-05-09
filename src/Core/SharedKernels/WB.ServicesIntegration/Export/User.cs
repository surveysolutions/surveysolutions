using System;

namespace WB.ServicesIntegration.Export
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public UserRoles[] Roles { get; set; } = new UserRoles[0];
    }
}
