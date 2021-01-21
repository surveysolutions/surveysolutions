using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.Aggregates
{
    public class ReadonlyUser
    {
        public ReadonlyUser()
        {
            this.RoleIds = new HashSet<Guid>();
        }

        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; }

        public virtual HqUserProfile ReadonlyProfile { get; protected set; }

        public virtual ISet<Guid> RoleIds { get; protected set; }
    }

    
}
