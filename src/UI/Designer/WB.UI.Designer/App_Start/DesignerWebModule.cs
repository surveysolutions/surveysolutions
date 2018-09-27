using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Exceptions;
using IRecipientNotifier = WB.Core.BoundedContexts.Designer.Services.IRecipientNotifier;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Modularity;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Designer.Api.WebTester;
using WB.UI.Designer.Code;
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
            registry.Bind<CustomHandleErrorFilter>();
            registry.Bind<CustomAuthorizeFilter>();
            //            registry.BindMvcFilter<CustomHandleErrorFilter>(FilterScope.Global, 20);
            //            registry.BindMvcFilter<CustomAuthorizeFilter>(FilterScope.Global, 20);

            registry.Bind<IJsonAllTypesSerializer, JsonAllTypesSerializer>();

            registry.Bind<IQuestionnairePackageComposer, QuestionnairePackageComposer>();
            registry.Bind<IArchiveUtils, ZipArchiveUtils>();
            registry.Bind<IMembershipHelper, MembershipHelper>();

            registry.Bind<IMembershipWebUser, MembershipWebUser>();
            registry.Bind<IMembershipWebServiceUser, MembershipWebServiceUser>();
            registry.Bind<IMembershipUserService, MembershipUserService>();

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

            // temp override registration
            registry.BindInPerUnitOfWorkScope<IUnitOfWork, UnitOfWork>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            System.Web.Mvc.GlobalFilters.Filters.Add(serviceLocator.GetInstance<CustomHandleErrorFilter> ());
            System.Web.Mvc.GlobalFilters.Filters.Add(serviceLocator.GetInstance<CustomAuthorizeFilter>());

            return Task.CompletedTask;
        }
    }
}
