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
            this.CreationDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Roles = new List<UserRoles>();
            this.DeviceChangingHistory = new List<DeviceInfo>();
        }

        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsLockedByHQ { get; set; }

        public bool IsLockedBySupervisor { get; set; }

        public string Password { get; set; }
        public Guid PublicKey { get; set; }
        public List<UserRoles> Roles { get; set; }
        public UserLight Supervisor { get; set; }
        public string UserName { get; set; }

        public bool IsHq()
        {
            return Roles.Any(role => role == UserRoles.Headquarter);
        }

        public bool IsAdmin()
        {
            return Roles.Any(role => role == UserRoles.Administrator);
        }

        public DateTime LastChangeDate { get; set; }

        public string DeviceId { get; set; }

        public List<DeviceInfo> DeviceChangingHistory { get; set; }

        public UserLight GetUseLight()
        {
            return new UserLight(this.PublicKey, this.UserName);
        }
    }
}