using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public static class RoleExtensions
    {
        private static readonly Dictionary<UserRoles, Guid> roleEnumToId = new Dictionary<UserRoles, Guid>
        {
            { UserRoles.Administrator, Guid.Parse("00000000000000000000000000000001") },
            { UserRoles.Supervisor, Guid.Parse("00000000000000000000000000000002") },
            { UserRoles.Interviewer, Guid.Parse("00000000000000000000000000000004") },
            { UserRoles.Headquarter, Guid.Parse("00000000000000000000000000000006") },
            { UserRoles.Observer, Guid.Parse("00000000000000000000000000000007") },
            { UserRoles.ApiUser, Guid.Parse("00000000000000000000000000000008") }
        };

        private static readonly Dictionary<Guid, UserRoles> roleIdToEnum = roleEnumToId.ToDictionary(x => x.Value, x => x.Key);

        public static Guid ToUserId(this UserRoles role) => roleEnumToId[role];
        public static UserRoles ToUserRole(this Guid role) => roleIdToEnum[role];
    }
}