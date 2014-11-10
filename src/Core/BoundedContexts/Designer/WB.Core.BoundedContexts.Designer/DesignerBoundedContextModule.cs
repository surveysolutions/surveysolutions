using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionDetailsViewMapper>().To<QuestionDetailsViewMapper>().InSingletonScope();
            this.Bind<IQuestionnaireExportService>().To<QuestionnaireExportService>().InSingletonScope();
            this.Bind<IQuestionnaireDocumentUpgrader>().To<QuestionnaireDocumentUpgrader>().InSingletonScope();
            this.Bind<IQuestionnaireEntityFactory>().To<QuestionnaireEntityFactory>().InSingletonScope();
            this.Bind<IQuestionnaireVersioner>().To<QuestionnaireVersioner>().InSingletonScope();
            this.Bind<INCalcToCSharpConverter>().To<NCalcToCSharpConverter>();

            this.Bind<IAsyncExecutor>().To<AsyncExecutor>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Unbind<IExpressionProcessor>();
            this.Bind<IExpressionProcessor>().To<RoslynExpressionProcessor>().InSingletonScope();

            DispatcherRegistryHelper.RegisterDenormalizer<AccountDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireSharedPersonsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireListViewItemDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<PdfQuestionnaireDenormalizer>(this.Kernel);

            this.Bind<IEventHandler>().To<QuestionnaireInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<ChaptersInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<QuestionsAndGroupsCollectionDenormalizer>().InSingletonScope();
            

            this.Kernel.RegisterFactory<QuestionnaireListViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireViewFactory>();
            this.Kernel.RegisterFactory<ChapterInfoViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireInfoViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireSharedPersonsFactory>();
            this.Kernel.RegisterFactory<AccountListViewFactory>();
            this.Kernel.RegisterFactory<AccountViewFactory>();
            this.Kernel.RegisterFactory<PdfQuestionnaireFactory>();
        }
    }
}
