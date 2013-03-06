using Designer.Web.Providers.CQRS.Accounts;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Roles.Commands
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(AccountAR))]
    public class CreateRoleCommand : CommandBase
    {
        public CreateRoleCommand() {}

        public CreateRoleCommand(string applicationName, string roleName, Guid publicKey)
        {
            ApplicationName = applicationName;
            RoleName = roleName;
            PublicKey = publicKey;
        }

        public string ApplicationName { set; get; }
        public string RoleName { set; get; }
        public Guid PublicKey { set; get; }
    }
}