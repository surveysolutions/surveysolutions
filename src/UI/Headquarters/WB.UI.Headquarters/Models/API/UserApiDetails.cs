using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Core.Supervisor.Views.User;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Headquarters.Models.API
{
    public class UserApiDetails 
    {
        public UserApiDetails(UserView userView)
        {
            if (userView == null)
                return;

            this.UserId = userView.PublicKey;
            this.UserName = userView.UserName;
            this.Email = userView.Email;
            this.CreationDate = userView.CreationDate;
            this.IsLocked = userView.IsLocked;
            this.Roles = userView.Roles;
        }

        [DataMember]
        public List<UserRoles> Roles { get; private set; }

        [DataMember]
        public bool IsLocked { get; private set; }

        [DataMember]
        public DateTime CreationDate { get; private set; }

        [DataMember]
        public string Email { get; private set; }

        [DataMember]
        public Guid UserId { get; private set; }

        [DataMember]
        public string UserName { get; private set; }
    }
}