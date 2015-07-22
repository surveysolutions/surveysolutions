using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Capi.Implementation.Authorization;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.UI.Capi.Authorization
{
    public class AndroidAuthentication : IDataCollectionAuthentication
    {
#warning ViewFactory should be used here
        private readonly IFilterableReadSideRepositoryReader<LoginDTO> documentStorage;
        private readonly IPasswordHasher passwordHasher;

        public AndroidAuthentication(IFilterableReadSideRepositoryReader<LoginDTO> documentStorage, IPasswordHasher passwordHasher)
        {
            this.documentStorage = documentStorage;
            this.passwordHasher = passwordHasher;
        }

        private UserLight currentUser;

        public UserLight CurrentUser
        {
            get { return this.currentUser; }
        }

        public Guid SupervisorId { get; private set; }


        public SyncCredentials? RequestSyncCredentials()
        {
            if (!this.IsLoggedIn)
                throw new InvalidOperationException("Please login first.");

            LoginDTO user = this.documentStorage.Filter(u => u.Login == this.CurrentUser.Name).FirstOrDefault();

            return new SyncCredentials(user.Login, user.Password);
        }

        public bool IsLoggedIn
        {
            get { return this.currentUser != null; }
        }

        public Task<bool> LogOnAsync(string userName, string password, bool wasPasswordHashed = false)
        {
            if (this.currentUser != null)
                throw new InvalidOperationException("Please logoff first.");
            try
            {
                var hash = wasPasswordHashed ? password : this.passwordHasher.Hash(password);
                var userNameToLower = userName.ToLower();

                LoginDTO user = this.documentStorage.Filter(u => u.Login == userNameToLower).FirstOrDefault();

                if (user == null || user.Password != hash || user.IsLockedBySupervisor || user.IsLockedByHQ)
                    return Task.FromResult(false);

                this.currentUser = new UserLight(Guid.Parse(user.Id), user.Login);

                Guid super;
                this.SupervisorId = Guid.TryParse(user.Supervisor, out super) ? super : Guid.NewGuid();
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task<List<string>> GetKnownUsers()
        {
            var logins = this.documentStorage
                .Filter(x => true)
                .Select(x => x.Login)
                .ToList();

            return Task.FromResult(logins);
        }

        public Task<Guid?> GetUserIdByLoginIfExists(string login)
        {
            var userIdAsString = this.documentStorage
                .Filter(x => x.Login == login)
                .Select(x => x.Id)
                .FirstOrDefault();

            Guid? userId = string.IsNullOrEmpty(userIdAsString) ? (Guid?)null : Guid.Parse(userIdAsString);
            return Task.FromResult(userId);
        }

        public void LogOff()
        {
            this.currentUser = null;
        }
    }
}