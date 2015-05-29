using System.Linq;
using System.Web.Mvc;
using Ncqrs;
using Ninject.Modules;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi.FilterBindingSyntax;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Mailers;
using WB.UI.Designer.WebServices;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Code
{
    public class DesignerRegistry : NinjectModule
    {
        public override void Load()
        {
            this.BindFilter<TransactionFilter>(FilterScope.First, 0)
              .WhenActionMethodHasNo<NoTransactionAttribute>();
            this.BindHttpFilter<ApiTransactionFilter>(System.Web.Http.Filters.FilterScope.Controller)
                 .When((controllerContext, actionDescriptor) => !actionDescriptor.GetCustomAttributes(typeof(NoTransactionAttribute)).Any());

            this.Bind<ICommandPreprocessor>().To<CommandPreprocessor>();
            this.Bind<IQuestionnaireHelper>().To<QuestionnaireHelper>();
            this.Bind<IVerificationErrorsMapper>().To<VerificationErrorsMapper>();
            this.Bind<ISystemMailer>().To<SystemMailer>();
            this.Bind<IPublicService>().To<PublicService>();
            this.Bind<IDynamicCompiler>().To<RoslynCompiler>();
            this.Bind<IExpressionReplacer>().To<ExpressionReplacer>();
            this.Bind<IExpressionProcessorGenerator>().To<QuestionnaireExpressionProcessorGenerator>();
            this.Bind<IChapterInfoViewFactory>().To<ChapterInfoViewFactory>();
            this.Bind<IQuestionnaireInfoFactory>().To<QuestionnaireInfoFactory>();
            this.Bind<IQuestionnaireInfoViewFactory>().To<QuestionnaireInfoViewFactory>();
        }
    }
}