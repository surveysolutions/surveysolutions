using System;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.User
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public UserRoles[] Roles { get; set; } = new UserRoles[0];
    }
}
