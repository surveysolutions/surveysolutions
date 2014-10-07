using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.UI.Shared.Web.MembershipProvider.Settings;
using WB.UI.Shared.Web.Modules;

namespace WB.UI.Designer
{
    using Ninject.Modules;
    using WB.UI.Shared.Web.MembershipProvider.Accounts;
    using WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class MembershipModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPasswordStrategy>().To<HashPasswordStrategy>().InSingletonScope();
            Bind<IPasswordPolicy>().ToMethod(_ => PasswordPolicyFactory.CreatePasswordPolicy()).InSingletonScope();
            Bind<IAccountRepository>().ToMethod(c => AccountRepositoryFactory.CreateRepository()).InSingletonScope();
            Bind<IRoleRepository>().To<CQRSRoleRepository>().InSingletonScope();
        }
    }
}