// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MembershipModule.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MembershipModule type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer
{
    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

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
                .ToConstructor(x => new CQRSAccountRepository(NcqrsEnvironment.Get<ICommandService>()))
                .InSingletonScope();
            Bind<IRoleRepository>()
                .ToConstructor(
                    x => new CQRSRoleRepository(NcqrsEnvironment.Get<ICommandService>()))
                .InSingletonScope();
        }
    }
}