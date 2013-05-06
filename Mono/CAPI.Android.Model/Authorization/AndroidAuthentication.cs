using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Main.Synchronization.Credentials;

namespace CAPI.Android.Core.Model.Authorization
{
    public class AndroidAuthentication : IAuthentication
    {
        private readonly IDenormalizerStorage<LoginDTO> _documentStorage;
        public AndroidAuthentication(IDenormalizerStorage<LoginDTO> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        private UserLight currentUser;
        #region Implementation of IAuthentication

        public UserLight CurrentUser
        {
            get { return currentUser; }
        }


        public SyncCredentials RequestSyncCredentials()
        {
            if(!IsLoggedIn)
                throw new InvalidOperationException("please logoin first");

            LoginDTO user =
                 _documentStorage.Query(
                     u =>
                     u.Login == CurrentUser.Name)
                                 .FirstOrDefault();
            return new SyncCredentials(user.Login, user.Password);
        }

        public bool IsLoggedIn { get { return currentUser != null; }
    }

        public bool LogOn(string userName, string password)
        {
            if (currentUser != null)
                throw new InvalidOperationException("please logoff first");
            try
            {
                var hash = SimpleHash.ComputeHash(password);

                LoginDTO user =
                    _documentStorage.Query(
                        u =>
                        u.Login == userName/* && u.Password == hash && !u.IsLocked*/)
                                    .FirstOrDefault();
                
                
              /*  UserView user =
                    CapiApplication.LoadView<UserViewInputModel, UserView>(
                        new UserViewInputModel(
                            userName.ToLower(),
                            // bad hack due to key insensitivity of login
                            SimpleHash.ComputeHash(password)));*/
                if (user == null || user.Password!=hash || user.IsLocked)
                    return false;

                currentUser = new UserLight(Guid.Parse(user.Id), user.Login);
              //  currentUser = new UserLight(Guid.NewGuid(), userName);
                return true;
            }
            catch(Exception e)
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