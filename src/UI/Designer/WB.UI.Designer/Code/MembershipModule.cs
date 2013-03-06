using Designer.Web.Providers.CQRS;
using Designer.Web.Providers.Membership;
using Designer.Web.Providers.Membership.PasswordStrategies;
using Designer.Web.Providers.Roles;
using Main.Core.View;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject.Modules;

namespace Designer.Web
{
    public class MembershipModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPasswordStrategy>().To<HashPasswordStrategy>().InSingletonScope();
            Bind<IPasswordPolicy>().ToConstant(PasswordPolicyFactory.CreatePasswordPolicy());
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