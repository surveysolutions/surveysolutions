using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.User
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(UserAR))]
    public class CreateUserCommand : CommandBase
    {
        public Guid PublicKey{get; set;}

        public string UserName
        {
            get; set;
        }

        public string Password
        {
            get;set;
        }

        public string Email
        {
            get; set;
        }

        public UserRoles[] Roles
        {
            get;set;
        }

        public bool IsLocked
        {
            get; set;
        }

        public CreateUserCommand(){}

        public CreateUserCommand(Guid publicKey, string userName, string password, string email, UserRoles[] roles, bool isLocked)
            : base(publicKey)
        {
            PublicKey = publicKey;
            Password = password;
            UserName = userName;
            Email = email;
            Roles = roles;
            IsLocked = isLocked;
        }

    }

}
