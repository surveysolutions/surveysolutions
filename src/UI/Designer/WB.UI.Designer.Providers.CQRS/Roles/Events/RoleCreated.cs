using System;

namespace WB.UI.Designer.Providers.CQRS.Roles.Events
{
    public class RoleCreated
    {
        public string ApplicationName { set; get; }
        public string RoleName { set; get; }
        public Guid PublicKey { set; get; }
    }
}
