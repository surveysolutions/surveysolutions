using System;
using System.Runtime.Serialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.Api
{
    public class UserApiItem
    {
        public UserApiItem(Guid id, string name, string email, DateTime creationDate, bool isLocked)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate;
            this.IsLocked = isLocked;
        }

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