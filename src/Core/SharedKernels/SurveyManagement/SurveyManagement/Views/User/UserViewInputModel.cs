using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
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

        public Guid? PublicKey { get; protected set; }

        public string UserEmail { get; protected set; }

        public string UserName { get; protected set; }
    }
}