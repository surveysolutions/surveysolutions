using System;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events.User;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAR : AggregateRootMappedByConvention
    {
        private Guid _publicKey;

        private string _userName;
        private string _email;
        private string _password;
        private bool _isLocked;
        private UserRoles[] _roles;
        private UserLight _supervisor;

        public UserAR() { }

        public UserAR(Guid publicKey, string userName, string password, string email, UserRoles[] roles,
            bool isLocked, UserLight supervisor)
            : base(publicKey)
        {

            //Check for uniqueness of person name and email!



            ApplyEvent(new NewUserCreated()
            {
                Name = userName,
                Password = password,
                Email = email,
                IsLocked = isLocked,
                Roles = roles,
                Supervisor = supervisor,
                PublicKey = publicKey
            });
        }

        // Event handler for the NewUserCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewUserCreated e)
        {
            _userName = e.Name;
            _email = e.Email;
            _password = e.Password;
            _isLocked = e.IsLocked;
            _roles = e.Roles;
            _supervisor = e.Supervisor;
        }

        public void ChangeUser(string email, bool isLocked, UserRoles[] roles)
        {
            ApplyEvent(new UserChanged()
            {
                Email = email,
                IsLocked = isLocked,
                Roles = roles,
                PublicKey = _publicKey
            });
        }


        public void SetUserLockState(bool isLocked)
        {
            ApplyEvent(new UserStatusChanged()
            {
                IsLocked = isLocked,
                PublicKey = _publicKey
            });
        }

        protected void OnSetUserLockState(UserStatusChanged e)
        {
            _isLocked = e.IsLocked;
        }

        protected void OnUserChange(UserChanged e)
        {
            _email = e.Email;
            _isLocked = e.IsLocked;
            _roles = e.Roles;

        }
    }
}
