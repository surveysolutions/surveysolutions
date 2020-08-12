using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserViewInputModel
    {
        public UserViewInputModel(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        public UserViewInputModel(string UserName, string UserEmail)
        {
            this.UserName = UserName;
            this.UserEmail = UserEmail;
        }

        public UserViewInputModel(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public UserViewInputModel()
        {
        }

        public Guid? PublicKey { get; set; }

        public string UserEmail { get;  set; }

        public string UserName { get;  set; }

        public string DeviceId { get;  set; }
    }
}
