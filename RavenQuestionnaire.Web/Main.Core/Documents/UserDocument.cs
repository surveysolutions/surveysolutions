// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserDocument.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the UserDocument type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    [DebuggerDisplay("User {UserName}")]
    public class UserDocument : IView
    {
        public UserDocument()
        {
            this.CreationDate = DateTime.Now;
            this.PublicKey = Guid.NewGuid();
            this.Roles = new List<UserRoles>();
            this.Location = new LocationDocument();
        }

        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsLocked { get; set; }
        public LocationDocument Location { get; set; }
        public string Password { get; set; }
        public Guid PublicKey { get; set; }
        public List<UserRoles> Roles { get; set; }
        public UserLight Supervisor { get; set; }
        public string UserName { get; set; }

        public DateTime LastChangeDate { get; set; }

        public UserLight GetUseLight()
        {
            return new UserLight(this.PublicKey, this.UserName);
        }
    }
}