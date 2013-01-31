using System;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Core.Model.Authorization
{
    public class AndroidAuthentication : IAuthentication
    {
        public AndroidAuthentication()
        {
        }

        private UserLight currentUser;
        #region Implementation of IAuthentication

        public UserLight CurrentUser
        {
            get { return currentUser; }
        }

        public bool IsLoggedIn { get { return currentUser != null; }
    }

        public bool LogOn(string userName, string password)
        {
            if (currentUser != null)
                throw new InvalidOperationException("please logoff first");
            try
            {
               /* UserView user =
                    CapiApplication.LoadView<UserViewInputModel, UserView>(
                        new UserViewInputModel(
                            userName.ToLower(),
                            // bad hack due to key insensitivity of login
                            SimpleHash.ComputeHash(password)));
                if (user == null || user.IsLocked)
                    return false;

                currentUser = new UserLight(user.PublicKey, user.UserName);*/
                currentUser = new UserLight(Guid.NewGuid(), userName);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public void LogOff()
        {
            currentUser = null;
        }

        #endregion
    }
}