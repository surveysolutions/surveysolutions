using System;

namespace Designer.Web.Providers.CQRS.Roles.Events
{
    public class RoleRemoved
    {
        public Guid PublicKey { get; set; }
    }
}
