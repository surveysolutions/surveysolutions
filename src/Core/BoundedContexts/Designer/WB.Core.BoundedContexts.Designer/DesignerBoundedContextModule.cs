using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.CommandBus;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.DependencyInjection;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.SurveySolutions.ReusableCategories;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : IAppModule
    {
        public void Load(IDependencyRegistry registry)
        {
            registry.BindAsSingleton<IEventTypeResolver, EventTypeResolver>(new EventTypeResolver(typeof(DesignerBoundedContextModule).Assembly));

            registry.Bind<IKeywordsProvider, KeywordsProvider>();
            registry.Bind<ISubstitutionService, SubstitutionService>();

            registry.Bind<IPatchGenerator, JsonPatchService>();
            registry.Bind<IPatchApplier, JsonPatchService>();
            registry.Bind<IPlainAggregateRootRepository<Questionnaire>, QuestionnaireRepository>();
            registry.Bind<IFindReplaceService, FindReplaceService>();

            registry.Bind<IQuestionnaireListViewFactory, QuestionnaireListViewFactory>();
            registry.Bind<IPublicFoldersStorage, PublicFoldersStorage>();
            registry.Bind<ICategoricalOptionsImportService, CategoricalOptionsImportService>();
            registry.Bind<IQuestionnaireChangeHistoryFactory, QuestionnaireChangeHistoryFactory>();
            registry.Bind<IQuestionnaireViewFactory, QuestionnaireViewFactory>();
            registry.Bind<IChapterInfoViewFactory, ChapterInfoViewFactory>();
            registry.Bind<IQuestionnaireInfoViewFactory, QuestionnaireInfoViewFactory>();
            registry.Bind<IAccountListViewFactory, AccountListViewFactory>();
            registry.Bind<IQuestionnaireCompilationVersionService, QuestionnaireCompilationVersionService>();
            registry.Bind<ITranslationsService, TranslationsService>();
            registry.Bind<IDesignerTranslationService, TranslationsService>();
            registry.Bind<ICategoriesVerifier, CategoriesVerifier>();
            registry.Bind<ICategoriesExtractFactory, CategoriesExtractFactory>();
            registry.Bind<ICategoriesService, CategoriesService>();
            registry.Bind<ITranslationsExportService, TranslationsExportService>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<ICategoriesExportService, CategoriesExportService>();

            registry.BindAsSingleton<IStringCompressor, JsonCompressor>();
            registry.Bind<ISerializer, NewtonJsonSerializer>();

            registry.BindAsSingleton<IExpressionProcessor, RoslynExpressionProcessor>();
            registry.BindAsSingleton<IDynamicCompilerSettingsProvider, DynamicCompilerSettingsProvider>();
            registry.Bind<ILookupTableService, LookupTableService>();
            registry.Bind<IAttachmentService, AttachmentService>();

            registry.Bind<IDesignerEngineVersionService, DesignerEngineVersionService>();
            registry.Bind<ICodeGeneratorV2, CodeGeneratorV2>();
            registry.Bind<IQuestionTypeToCSharpTypeMapper, QuestionTypeToCSharpTypeMapper>();
            registry.Bind<ICodeGenerationModelsFactory, CodeGenerationModelsFactory>();
            registry.Bind(typeof(ITopologicalSorter<>), typeof(TopologicalSorter<>));
            
            registry.Bind(typeof(IPlainKeyValueStorage<>), typeof(DesignerKeyValueStorage<>));
            registry.Bind(typeof(IEntitySerializer<>), typeof(EntitySerializer<>));
            registry.Bind(typeof(IPlainAggregateRootRepository), typeof(QuestionnaireRepository));
            registry.Bind<Questionnaire, Questionnaire>();
            registry.Bind<ListViewPostProcessor, ListViewPostProcessor>();
            registry.Bind<HistoryPostProcessor, HistoryPostProcessor>();
            registry.Bind<SearchPostProcessors, SearchPostProcessors>();
            registry.Bind<ResourcesPreProcessor, ResourcesPreProcessor>();
            registry.Bind<ResourcesPostProcessor, ResourcesPostProcessor>();
            registry.Bind<CopyPastePreProcessor, CopyPastePreProcessor>();

            registry.Bind<IDesignerQuestionnaireStorage, DesignerQuestionnaireStorage>();
            registry.Bind<IUserManager, DesignerUserManager>();

            registry.Bind<IAggregateRootPrototypeService, DummyAggregateRootPrototypeService>();
            registry.Bind<IAggregateRootPrototypePromoterService, DummyAggregateRootPrototypePromoterService>();
        }

        public Task InitAsync(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<CloneQuestionnaire>((command, aggregate) => aggregate.CloneQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId, command.QuestionnaireId, command.Source), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .InitializesWith<CreateQuestionnaire>((command, aggregate) => aggregate.CreateQuestionnaire(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .InitializesWith<ImportQuestionnaire>((command, aggregate) => aggregate.ImportQuestionnaire(command.ResponsibleId, command.Source), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteQuestionnaire>((command, aggregate) => aggregate.DeleteQuestionnaire(), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<ResourcesPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<RevertVersionQuestionnaire>((command, aggregate) => aggregate.RevertVersion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<ImportQuestionnaireToHq>((command, aggregate) => {}, config => config.PostProcessBy<HistoryPostProcessor>())
                .Handles<MigrateToNewVersion>((command, aggregate) => aggregate.MigrateToNewVersion(command), config => config.PostProcessBy<HistoryPostProcessor>())
                // Macro
                .Handles<AddMacro>(aggregate => aggregate.AddMacro, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteMacro>(aggregate => aggregate.DeleteMacro, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateMacro>(aggregate => aggregate.UpdateMacro, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // LookupTable
                .Handles<AddLookupTable>(aggregate => aggregate.AddLookupTable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateLookupTable>(aggregate => aggregate.UpdateLookupTable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteLookupTable>(aggregate => aggregate.DeleteLookupTable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Attachment
                .Handles<AddOrUpdateAttachment>(aggregate => aggregate.AddOrUpdateAttachment, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteAttachment>(aggregate => aggregate.DeleteAttachment, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Translation
                .Handles<AddOrUpdateTranslation>(aggregate => aggregate.AddOrUpdateTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteTranslation>(aggregate => aggregate.DeleteTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<SetDefaultTranslation>(aggregate => aggregate.SetDefaultTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Categories
                .Handles<AddOrUpdateCategories>(aggregate => aggregate.AddOrUpdateCategories, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteCategories>(aggregate => aggregate.DeleteCategories, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                // Metadata
                .Handles<UpdateMetadata>(aggregate => aggregate.UpdateMetaInfo, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Group
                .Handles<AddGroup>((command, aggregate) => aggregate.AddGroupAndMoveIfNeeded(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.Index), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteGroup>((command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<MoveGroup>((command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateGroup>((command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.DisplayMode), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                // Questions
                .Handles<MoveQuestion>((command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<AddDefaultTypeQuestion>((command, aggregate) => aggregate.AddDefaultTypeQuestionAdnMoveIfNeeded(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteQuestion>((command, aggregate) => aggregate.DeleteQuestion(command.QuestionId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateCascadingComboboxOptions>((command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateDateTimeQuestion>(aggregate => aggregate.UpdateDateTimeQuestion, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateFilteredComboboxOptions>((command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<HistoryPostProcessor>().PostProcessBy<ListViewPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<ReplaceOptionsWithClassification>((command, aggregate) => aggregate.ReplaceOptionsWithClassification(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateGpsCoordinatesQuestion>((command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateMultimediaQuestion>((command, aggregate) => aggregate.UpdateMultimediaQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateMultiOptionQuestion>((command, aggregate) => aggregate.UpdateMultiOptionQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateAreaQuestion>((command, aggregate) => aggregate.UpdateAreaQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateAudioQuestion>((command, aggregate) => aggregate.UpdateAudioQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())

                .Handles<UpdateNumericQuestion>((command, aggregate) => aggregate.UpdateNumericQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateQRBarcodeQuestion>((command, aggregate) => aggregate.UpdateQRBarcodeQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateQuestionnaire>(aggregate => aggregate.UpdateQuestionnaire, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateSingleOptionQuestion>((command, aggregate) => aggregate.UpdateSingleOptionQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateTextListQuestion>((command, aggregate) => aggregate.UpdateTextListQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<UpdateTextQuestion>((command, aggregate) => aggregate.UpdateTextQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<ReplaceTextsCommand>((command, aggregate) => aggregate.ReplaceTexts(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                // Copy-Paste
                .Handles<PasteAfter>(aggregate => aggregate.PasteAfter, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>().PreProcessBy<CopyPastePreProcessor>())
                .Handles<PasteInto>(aggregate => aggregate.PasteInto, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>().PreProcessBy<CopyPastePreProcessor>())
                // Static text
                .Handles<AddStaticText>(aggregate => aggregate.AddStaticTextAndMoveIfNeeded, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<MoveStaticText>((command, aggregate) => aggregate.MoveStaticText(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateStaticText>(aggregate => aggregate.UpdateStaticText, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteStaticText>((command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>().PostProcessBy<SearchPostProcessors>())
                
                // Variable
                .Handles<AddVariable>(aggregate => aggregate.AddVariableAndMoveIfNeeded, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<MoveVariable>((command, aggregate) => aggregate.MoveVariable(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateVariable>(aggregate => aggregate.UpdateVariable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<SearchPostProcessors>())
                .Handles<DeleteVariable>((command, aggregate) => aggregate.DeleteVariable(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>().PostProcessBy<SearchPostProcessors>())
                
                // Sharing
                .Handles<AddSharedPersonToQuestionnaire>(
                    (command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.EmailOrLogin, command.ShareType, command.ResponsibleId), 
                    config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())

                .Handles<RemoveSharedPersonFromQuestionnaire>(
                    (command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId),
                    config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())

                .Handles<PassOwnershipFromQuestionnaire>(
                    (command, aggregate) => aggregate.TransferOwnership(command.ResponsibleId, command.NewOwnerId, command.OwnerEmail, command.NewOwnerEmail),
                    config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>());

            return Task.CompletedTask;
        }        
    }
}
