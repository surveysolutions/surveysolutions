using System.Web.Mvc;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using WB.Core.GenericSubdomains.Native;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Exceptions;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer
{
    /// <summary>
    /// The main module.
    /// </summary>
    public class MainModule : NinjectModule
    {
        public override void Load()
        {
            //this.Bind<ILog>().ToConstant(new Log()).InSingletonScope();
            this.BindFilter<CustomHandleErrorFilter>(FilterScope.Global, 0).InSingletonScope();
            this.BindFilter<CustomAuthorizeFilter>(FilterScope.Controller, 0).WhenControllerHas<CustomAuthorizeAttribute>().InSingletonScope();
            this.Bind<JsonUtilsSettings>().ToSelf().InSingletonScope();
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<JsonCompressor>().InSingletonScope();
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
