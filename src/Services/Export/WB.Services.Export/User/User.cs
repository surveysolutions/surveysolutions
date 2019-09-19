using System;
using WB.Services.Export.Interview.Entities;

namespace WB.Services.Export.User
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public UserRoles[] Roles { get; set; }
    }
}
