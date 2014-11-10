using System;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Entities.SubEntities;
using Main.Core.Utility;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.Authorization
{
    public class AndroidAuthentication : IDataCollectionAuthentication
    {
        #warning ViewFactory should be used here
        private readonly IFilterableReadSideRepositoryReader<LoginDTO> documentStorage;

        public AndroidAuthentication(IFilterableReadSideRepositoryReader<LoginDTO> documentStorage)
        {
            this.documentStorage = documentStorage;
        }

        private UserLight currentUser;
        #region Implementation of IAuthentication

        public UserLight CurrentUser
        {
            get { return currentUser; }
        }

        public Guid SupervisorId { get; private set; }


        public SyncCredentials? RequestSyncCredentials()
        {
            if(!IsLoggedIn)
                throw new InvalidOperationException("Please login first.");

            LoginDTO user = this.documentStorage.Filter(u =>u.Login == CurrentUser.Name).FirstOrDefault();

            return new SyncCredentials(user.Login, user.Password);
        }

        public bool IsLoggedIn { get { return currentUser != null; }
    }

        public bool LogOn(string userName, string password, bool wasPasswordHashed = false)
        {
            if (currentUser != null)
                throw new InvalidOperationException("Please logoff first.");
            try
            {
                var hash = wasPasswordHashed ? password : SimpleHash.ComputeHash(password);
                var userNameToLower = userName.ToLower();

                LoginDTO user = this.documentStorage.Filter(u => u.Login == userNameToLower).FirstOrDefault();

                if (user == null || user.Password != hash || user.IsLockedBySupervisor || user.IsLockedByHQ)
                    return false;

                currentUser = new UserLight(Guid.Parse(user.Id), user.Login);

                Guid super;
                this.SupervisorId = Guid.TryParse(user.Supervisor, out super) ? super : Guid.NewGuid();
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