using System;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class UserViewInputModel
    {
        public UserViewInputModel(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }
        
        public UserViewInputModel(string userName, string userEmail)
        {
            this.UserName = userName;
            this.UserEmail = userEmail;
        }
        
        public Guid? PublicKey { get; protected set; }

        public string UserEmail { get; protected set; }

        public string UserName { get; protected set; }
    }
}