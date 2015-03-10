using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Views
{
    [DebuggerDisplay("User {UserName}")]
    public class UserDocument : IView
    {
        public UserDocument()
        {
            this.Roles = new List<UserRoles>();
            this.DeviceChangingHistory = new List<DeviceInfo>();
        }

        public virtual string UserId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string Email { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual bool IsLockedByHQ { get; set; }
        public virtual bool IsLockedBySupervisor { get; set; }
        public virtual string Password { get; set; }
        public virtual Guid PublicKey { get; set; }
        public virtual IList<UserRoles> Roles { get; set; }
        public virtual UserLight Supervisor { get; set; }
        public virtual string UserName { get; set; }
        public virtual DateTime LastChangeDate { get; set; }
        public virtual string DeviceId { get; set; }
        public virtual IList<DeviceInfo> DeviceChangingHistory { get; set; }

        public virtual bool IsHq()
        {
            return this.Roles.Any(role => role == UserRoles.Headquarter);
        }

        public virtual UserLight GetUseLight()
        {
            return new UserLight(this.PublicKey, this.UserName);
        }
    }
}