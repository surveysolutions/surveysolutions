using System.Web.Mvc;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Exceptions;
using WB.UI.Shared.Web.Membership;
using IRecipientNotifier = WB.UI.Designer.Code.IRecipientNotifier;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;

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
            this.BindFilter<CustomAuthorizeFilter>(FilterScope.Global, 0).InSingletonScope();

            this.Bind<ISerializer>().ToMethod((ctx) => new NewtonJsonSerializer());
            this.Bind<IJsonAllTypesSerializer>().ToMethod((ctx) => new JsonAllTypesSerializer());

            this.Bind<IStringCompressor>().To<JsonCompressor>().InSingletonScope();
            this.Bind<IArchiveUtils>().To<ZipArchiveUtils>();
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

            this.Bind<IRecipientNotifier>().To<MailNotifier>();
        }
    }
}
