﻿using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.PasswordStrategies;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.MembershipProvider.Settings;

namespace WB.UI.Designer.Code
{
    public class MembershipModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IPasswordStrategy, HashPasswordStrategy>();
            registry.BindToConstant<IPasswordPolicy>(PasswordPolicyFactory.CreatePasswordPolicy);
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
