using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserViewInputModel
    {
        public UserViewInputModel(string id)
        {
            UserId = IdUtil.CreateUserId(id);
        }

        public UserViewInputModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }
        public string UserId { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
    }
}
