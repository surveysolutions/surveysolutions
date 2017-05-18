using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class UserApiItem
    {
        public UserApiItem(Guid id, string name, string email, DateTime creationDate, bool isLocked, string deviceId)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate;
            this.IsLocked = isLocked;
            this.DeviceId = deviceId;
        }

        [DataMember]
        [Required]
        public bool IsLocked { get; private set; }

        [DataMember]
        [Required]
        public DateTime CreationDate { get; private set; }

        [DataMember]
        public string Email { get; private set; }

        [DataMember]
        public string DeviceId { get; private set; }

        [DataMember]
        [Required]
        public Guid UserId { get; private set; }

        [DataMember]
        [Required]
        public string UserName { get; private set; }
    }
}