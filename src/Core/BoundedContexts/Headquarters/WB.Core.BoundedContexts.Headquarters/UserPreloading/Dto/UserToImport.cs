using System;
using System.Runtime.Serialization;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class UserToImport
    {
        public virtual int Id { get; set; }
        public virtual string Login { get; set; }
        public virtual string Password { get; set; }
        public virtual string Email { get; set; }
        public virtual string FullName { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Role { get; set; }
        public virtual string Supervisor { get; set; }

        [IgnoreDataMember]
        public virtual UserRoles UserRole
        {
            get
            {
                if ("supervisor".Equals(this.Role, StringComparison.InvariantCultureIgnoreCase))
                    return UserRoles.Supervisor;
                if ("interviewer".Equals(this.Role, StringComparison.InvariantCultureIgnoreCase))
                    return UserRoles.Interviewer;

                return 0;
            }
        }
    }
}