using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View.User;
using Main.DenormalizerStorage;

namespace CAPI.Android.Core.Model.Authorization
{
    public class AndroidAuthentication : IAuthentication
    {
        private readonly IDenormalizerStorage<UserView> _documentStorage;
        public AndroidAuthentication(IDenormalizerStorage<UserView> documentStorage)
        {
            _documentStorage = documentStorage;
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
                UserView user =
                    _documentStorage.Query().FirstOrDefault(
                        u =>
                        u.UserName.ToLower() == userName.ToLower() && u.Password == SimpleHash.ComputeHash(password));
                
                
              /*  UserView user =
                    CapiApplication.LoadView<UserViewInputModel, UserView>(
                        new UserViewInputModel(
                            userName.ToLower(),
                            // bad hack due to key insensitivity of login
                            SimpleHash.ComputeHash(password)));*/
                if (user == null || user.IsLocked)
                    return false;

                currentUser = new UserLight(user.PublicKey, user.UserName);
              //  currentUser = new UserLight(Guid.NewGuid(), userName);
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