using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.PasswordStrategies;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Settings;

namespace WB.UI.Designer.Code
{
    public class MembershipModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPasswordStrategy>().To<HashPasswordStrategy>().InSingletonScope();
            this.Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();
            this.Bind<IAccountRepository>().ToMethod(c => AccountRepositoryFactory.CreateRepository()).InSingletonScope();
            this.Bind<IRoleRepository>().To<CQRSRoleRepository>().InSingletonScope();
        }
    }
}