using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Users.Denormalizers
{
    public static class RolesExtensions
    {
        public static bool HasSupervisorApplicationRole(this IEnumerable<UserRoles> roles)
        {
            if (roles == null) throw new ArgumentNullException("roles");

            return roles.Contains(UserRoles.Supervisor) || roles.Contains(UserRoles.Operator);
        }
    }
}