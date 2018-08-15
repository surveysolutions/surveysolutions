using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Exceptions;
using IRecipientNotifier = WB.Core.BoundedContexts.Designer.Services.IRecipientNotifier;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Modules;

namespace WB.UI.Designer
{
    /// <summary>
    /// The main module.
    /// </summary>
    public class DesignerWebModule : IWebModule
    {
        public void Load(IWebIocRegistry registry)
        {
            //registry.Bind<ILog>().ToConstant(new Log()).InSingletonScope();
            registry.BindMvcFilterInSingletonScope<CustomHandleErrorFilter>(FilterScope.Global, 0);
            registry.BindMvcFilterInSingletonScope<CustomAuthorizeFilter>(FilterScope.Global, 0);

            registry.BindToMethod<IJsonAllTypesSerializer>(() => new JsonAllTypesSerializer());

            registry.BindAsSingleton<IQuestionnairePackageComposer, QuestionnairePackageComposer>();
            registry.Bind<IArchiveUtils, ZipArchiveUtils>();
            registry.BindToConstant<IMembershipHelper>(() => new MembershipHelper());
            registry.BindToConstructorInSingletonScope<IMembershipWebUser>(x => new MembershipWebUser(x.Inject<IMembershipHelper>()));
            registry.BindToConstructorInSingletonScope<IMembershipWebServiceUser>(x => new MembershipWebServiceUser(x.Inject<IMembershipHelper>()));
            registry.BindToConstructorInSingletonScope<IMembershipUserService>(x =>
                    new MembershipUserService(
                        x.Inject<IMembershipHelper>(),
                        x.Inject<IMembershipWebUser>(),
                        x.Inject<IMembershipWebServiceUser>()));

            registry.Bind<IRecipientNotifier, MailNotifier>();

            registry.BindToMethod<WebTesterSettings>(() =>
            {
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;
                return new WebTesterSettings
                {
                    ExpirationAmountMinutes = appSettings["WebTester.ExpirationAmountMinutes"].AsInt(),
                    BaseUri = appSettings["WebTester.BaseUri"]
                };
            });

            registry.BindAsSingleton<IWebTesterService, WebTesterService>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
