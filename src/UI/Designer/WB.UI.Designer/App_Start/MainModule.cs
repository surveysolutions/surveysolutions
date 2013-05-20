namespace WB.UI.Designer.App_Start
{
    using System.Web.Mvc;

    using Ncqrs.Commanding.ServiceModel;
    using Ninject.Modules;
    using Ninject.Web.Mvc.FilterBindingSyntax;

    using WB.UI.Designer.Exceptions;
    using WB.UI.Designer.Utilities.Compression;
    using WB.UI.Shared.Log;
    using WB.UI.Shared.NLog;
    using WB.UI.Shared.Web.Membership;

    /// <summary>
    /// The main module.
    /// </summary>
    public class MainModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ILog>().ToConstant(new Log()).InSingletonScope();
            this.BindFilter<CustomHandleErrorFilter>(FilterScope.Global, 0).InSingletonScope();
            this.BindFilter<CustomAuthorizeFilter>(FilterScope.Controller, 0).WhenControllerHas<CustomAuthorizeAttribute>().InSingletonScope();
            this.Bind<ICommandService>().ToConstant(Ncqrs.NcqrsEnvironment.Get<ICommandService>());
            this.Bind<IZipUtils>().ToConstant(new ZipUtils()).InSingletonScope();
            this.Bind<IMembershipHelper>().ToConstant(new MembershipHelper()).InSingletonScope();
            this.Bind<IMembershipWebUser>()
                .ToConstructor(x => new MembershipWebUser(x.Inject<IMembershipHelper>()))
                .InSingletonScope();
            this.Bind<IMembershipWebServiceUser>()
                .ToConstructor(x => new MembershipWebServiceUser(x.Inject<IMembershipHelper>()))
                .InSingletonScope();
            this.Bind<IMembershipUserService>()
                .ToConstructor(
                    x =>
                    new MembershipUserService(
                        x.Inject<IMembershipHelper>(),
                        x.Inject<IMembershipWebUser>(),
                        x.Inject<IMembershipWebServiceUser>()))
                .InSingletonScope();
        }
    }
}
