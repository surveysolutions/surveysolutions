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
    using Main.Core.View;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject.Modules;

    using WB.UI.Designer.Providers.CQRS;
    using WB.UI.Designer.Providers.Membership;
    using WB.UI.Designer.Providers.Membership.PasswordStrategies;
    using WB.UI.Designer.Providers.Roles;

    public class MembershipModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPasswordStrategy>().To<HashPasswordStrategy>().InSingletonScope();
            Bind<IPasswordPolicy>().ToConstant(PasswordPolicyFactory.CreatePasswordPolicy(AppSettings.Instance.IsLockingAccountPolicyForced));
            Bind<IAccountRepository>()
                .ToConstructor(x => new CQRSAccountRepository(x.Inject<IViewRepository>(),
                                                              NcqrsEnvironment.Get<ICommandService>()))
                .InSingletonScope();
            Bind<IRoleRepository>()
                .ToConstructor(
                    x => new CQRSRoleRepository(x.Inject<IViewRepository>(), NcqrsEnvironment.Get<ICommandService>()))
                .InSingletonScope();
        }
    }
}