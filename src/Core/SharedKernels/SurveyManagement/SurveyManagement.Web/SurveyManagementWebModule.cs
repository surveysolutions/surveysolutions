using System.Linq;
using System.Web.Mvc;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandDeserialization;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Core.SharedKernels.SurveyManagement.Web
{
    public class SurveyManagementWebModule : NinjectModule
    {
        public override void Load()
        {
            this.BindFilter<TransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindHttpFilter<ApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                 .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());

            this.BindFilter<PlainTransactionFilter>(FilterScope.First, 0)
                .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindHttpFilter<PlainApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());

            //this.Bind<IUserWebViewFactory>().To<UserWebViewFactory>(); // binded automatically but should not
            this.Bind<ICommandDeserializer>().To<SurveyManagementCommandDeserializer>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
        }
    }
}