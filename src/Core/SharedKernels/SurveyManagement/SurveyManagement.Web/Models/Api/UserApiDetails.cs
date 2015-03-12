using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
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
            this.IsLocked = userView.IsLockedBySupervisor || userView.IsLockedByHQ;
            this.Roles = userView.Roles.ToList();
        }

        [DataMember]
        public IList<UserRoles> Roles { get; private set; }

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