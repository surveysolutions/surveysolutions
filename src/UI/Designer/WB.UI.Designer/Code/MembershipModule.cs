namespace WB.UI.Designer
{
    using Ninject.Modules;

    using WB.UI.Designer.Providers.CQRS;
    using WB.UI.Shared.Web.MembershipProvider.Accounts;
    using WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class MembershipModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPasswordStrategy>().To<HashPasswordStrategy>().InSingletonScope();
            Bind<IPasswordPolicy>().ToConstant(PasswordPolicyFactory.CreatePasswordPolicy());
            Bind<IAccountRepository>()
                .ToConstant(AccountRepositoryFactory.CreateRepository()).InSingletonScope();
            Bind<IRoleRepository>().To<CQRSRoleRepository>().InSingletonScope();
        }
    }
}