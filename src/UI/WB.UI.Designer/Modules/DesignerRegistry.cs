using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Designer.Code;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Modules
{
    public class DesignerRegistry : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.Bind<ICommandInflater, CommandInflater>();
            registry.Bind<IQuestionnaireHelper, QuestionnaireHelper>();
            registry.Bind<IVerificationErrorsMapper, VerificationErrorsMapper>();
            registry.Bind<IDynamicCompiler, RoslynCompiler>();
            registry.Bind<IMacrosSubstitutionService, MacrosSubstitutionService>();
            registry.Bind<IExpressionProcessorGenerator, QuestionnaireExpressionProcessorGenerator>();
            registry.Bind<IExpressionsGraphProvider, ExpressionsGraphProvider>();
            registry.Bind<IExpressionsPlayOrderProvider, ExpressionsPlayOrderProvider>();
            registry.Bind<IQuestionnaireInfoFactory, QuestionnaireInfoFactory>();
            registry.Bind<ICommentsService, CommentsService>();
            registry.Bind<IPdfFactory, PdfFactory>();
            registry.Bind<ICommandsMonitoring, TraceCommandsMonitoring>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
