using WB.UI.Designer.Providers.CQRS.Accounts;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Roles.Commands
{
    using WB.UI.Designer.Providers.CQRS.Accounts;

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