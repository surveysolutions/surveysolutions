using System;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events.User;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAR :  AggregateRootMappedByConvention
    {
        private string _userName;
        private string _email;
        private string _password;
        private bool _isLocked;
        private UserRoles[] _roles;
        

        public UserAR()
        {
        }

        public UserAR(Guid publicKey, string userName, string password, string email, UserRoles[] roles, bool isLocked)
            : base(publicKey)
        {

            //Check for unique of person


            ApplyEvent(new NewUserCreated()
                           {
                               Name = userName,
                               Password = password,
                               Email = email,
                               IsLocked = isLocked,
                               Roles = roles
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

        }




    }
}
