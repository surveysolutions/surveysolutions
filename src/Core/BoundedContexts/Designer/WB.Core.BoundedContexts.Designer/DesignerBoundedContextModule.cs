using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
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
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors;
using WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.TopologicalSorter;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Questionnaire;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage;

namespace WB.Core.BoundedContexts.Designer
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DesignerBoundedContextModule : IModule
    {
        private readonly ICompilerSettings compilerSettings;

        public DesignerBoundedContextModule(ICompilerSettings compilerSettings)
        {
            this.compilerSettings = compilerSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IEventTypeResolver>(() => new EventTypeResolver(typeof(DesignerBoundedContextModule).Assembly));

            registry.Bind<IKeywordsProvider, KeywordsProvider>();
            registry.Bind<ISubstitutionService, SubstitutionService>();

            registry.Bind<IPlainAggregateRootRepository<User>, UserRepository>();
            registry.Bind<IPatchGenerator, JsonPatchService>();
            registry.Bind<IPatchApplier, JsonPatchService>();
            registry.Bind<IPlainAggregateRootRepository<Questionnaire>, QuestionnaireRepository>();
            registry.BindToMethod<IFindReplaceService>(c => new FindReplaceService(c.Get<IPlainAggregateRootRepository<Questionnaire>>()));

            registry.Bind<IQuestionnaireListViewFactory, QuestionnaireListViewFactory>();
            registry.Bind<IPublicFoldersStorage, PublicFoldersStorage>();
            registry.Bind<IQuestionnaireChangeHistoryFactory, QuestionnaireChangeHistoryFactory>();
            registry.Bind<IQuestionnaireViewFactory, QuestionnaireViewFactory>();
            registry.Bind<IChapterInfoViewFactory, ChapterInfoViewFactory>();
            registry.Bind<IQuestionnaireInfoViewFactory, QuestionnaireInfoViewFactory>();
            registry.Bind<IAccountListViewFactory, AccountListViewFactory>();
            registry.Bind<IAccountViewFactory, AccountViewFactory>();
            registry.Bind<IAllowedAddressService, AllowedAddressService>();
            registry.Bind<IQuestionnaireCompilationVersionService, QuestionnaireCompilationVersionService>();
            registry.Bind<IIpAddressProvider, IpAddressProvider>();
            registry.Bind<ITranslationsService, TranslationsService>();
            registry.Bind<ITranslationsExportService, TranslationsExportService>();
            registry.Bind<IQuestionnaireTranslator, QuestionnaireTranslator>();
            registry.Bind<IAccountRepository, DesignerAccountRepository>();

            registry.BindAsSingleton<IStringCompressor, JsonCompressor>();
            registry.Bind<ISerializer, NewtonJsonSerializer>();
            registry.BindInPerLifetimeScope<OriginalQuestionnaireStorage, OriginalQuestionnaireStorage>();

            registry.BindAsSingleton<IExpressionProcessor, RoslynExpressionProcessor>();
            registry.BindToConstant<ICompilerSettings>(() => this.compilerSettings);
            registry.Bind<IDynamicCompilerSettingsProvider, DynamicCompilerSettingsProvider>();
            registry.Bind<ILookupTableService, LookupTableService>();
            registry.Bind<IAttachmentService, AttachmentService>();

            registry.Bind<IDesignerEngineVersionService, DesignerEngineVersionService>();
            registry.Bind<ICodeGenerator, CodeGenerator>();
            registry.Bind<ICodeGeneratorV2, CodeGeneratorV2>();
            registry.Bind<IQuestionTypeToCSharpTypeMapper, QuestionTypeToCSharpTypeMapper>();
            registry.Bind<ICodeGenerationModelsFactory, CodeGenerationModelsFactory>();
            registry.Bind(typeof(ITopologicalSorter<>), typeof(TopologicalSorter<>));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            CommandRegistry
                .Setup<User>()
                .ResolvesIdFrom<UserCommand>(command => command.UserId)
                .InitializesWith<RegisterUser>((command, aggregate) => aggregate.Register(command.ApplicationName, command.UserName, command.Email, command.UserId, command.Password, command.PasswordSalt, command.IsConfirmed, command.ConfirmationToken))
                .Handles<AssignUserRole>((command, aggregate) => aggregate.AddRole(command.Role))
                .Handles<ChangeUserPassword>((command, aggregate) => aggregate.ChangePassword(command.Password))
                .Handles<ChangeSecurityQuestion>((command, aggregate) => aggregate.ChangePasswordQuestionAndAnswer(command.PasswordQuestion, command.PasswordAnswer))
                .Handles<SetPasswordResetToken>((command, aggregate) => aggregate.ChangePasswordResetToken(command.PasswordResetToken, command.PasswordResetExpirationDate))
                .Handles<ConfirmUserAccount>((command, aggregate) => aggregate.Confirm())
                .Handles<DeleteUserAccount>((command, aggregate) => aggregate.Delete())
                .Handles<LockUserAccount>((command, aggregate) => aggregate.Lock())
                .Handles<RegisterFailedLogin>((command, aggregate) => aggregate.LoginFailed())
                .Handles<RemoveUserRole>((command, aggregate) => aggregate.RemoveRole(command.Role))
                .Handles<ResetUserPassword>((command, aggregate) => aggregate.ResetPassword(command.Password, command.PasswordSalt))
                .Handles<UnlockUserAccount>((command, aggregate) => aggregate.Unlock())
                .Handles<UpdateUserAccount>((command, aggregate) => aggregate.Update(command.UserName, command.IsLockedOut, command.PasswordQuestion, command.Email, command.IsConfirmed, command.Comment, command.CanImportOnHq));

            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>(command => command.QuestionnaireId)
                .InitializesWith<CloneQuestionnaire>((command, aggregate) => aggregate.CloneQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId, command.QuestionnaireId, command.Source), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .InitializesWith<CreateQuestionnaire>((command, aggregate) => aggregate.CreateQuestionnaire(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .InitializesWith<ImportQuestionnaire>((command, aggregate) => aggregate.ImportQuestionnaire(command.ResponsibleId, command.Source), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteQuestionnaire>((command, aggregate) => aggregate.DeleteQuestionnaire(), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PostProcessBy<ResourcesPostProcessor>())
                .Handles<RevertVersionQuestionnaire>((command, aggregate) => aggregate.RevertVersion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
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
                .Handles<AddOrUpdateTranslation>(aggregate => aggregate.AddOrUpdateTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteTranslation>(aggregate => aggregate.DeleteTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<SetDefaultTranslation>(aggregate => aggregate.SetDefaultTranslation, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Metadata
                .Handles<UpdateMetadata>(aggregate => aggregate.UpdateMetaInfo, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Group
                .Handles<AddGroup>((command, aggregate) => aggregate.AddGroupAndMoveIfNeeded(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.Index), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteGroup>((command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>())
                .Handles<MoveGroup>((command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateGroup>((command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Questions
                .Handles<MoveQuestion>((command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<AddDefaultTypeQuestion>((command, aggregate) => aggregate.AddDefaultTypeQuestionAdnMoveIfNeeded(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteQuestion>((command, aggregate) => aggregate.DeleteQuestion(command.QuestionId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>())
                .Handles<UpdateCascadingComboboxOptions>((command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<ListViewPostProcessor>())
                .Handles<UpdateDateTimeQuestion>(aggregate => aggregate.UpdateDateTimeQuestion, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateFilteredComboboxOptions>((command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<ListViewPostProcessor>())
                .Handles<UpdateGpsCoordinatesQuestion>((command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateMultimediaQuestion>((command, aggregate) => aggregate.UpdateMultimediaQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateMultiOptionQuestion>((command, aggregate) => aggregate.UpdateMultiOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToEntityId, command.AreAnswersOrdered, command.MaxAllowedAnswers, command.YesNoView, command.ValidationConditions, command.LinkedFilterExpression, command.Properties), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateAreaQuestion>((command, aggregate) => aggregate.UpdateAreaQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateAudioQuestion>((command, aggregate) => aggregate.UpdateAudioQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())

                .Handles<UpdateNumericQuestion>((command, aggregate) => aggregate.UpdateNumericQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateQRBarcodeQuestion>((command, aggregate) => aggregate.UpdateQRBarcodeQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateQuestionnaire>(aggregate => aggregate.UpdateQuestionnaire, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateSingleOptionQuestion>((command, aggregate) => aggregate.UpdateSingleOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsPreFilled, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToEntityId, command.IsFilteredCombobox, command.CascadeFromQuestionId, command.ValidationConditions, command.LinkedFilterExpression, command.Properties), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateTextListQuestion>((command, aggregate) => aggregate.UpdateTextListQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateTextQuestion>((command, aggregate) => aggregate.UpdateTextQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<ReplaceTextsCommand>((command, aggregate) => aggregate.ReplaceTexts(command), config => config.PostProcessBy<HistoryPostProcessor>())
                // Copy-Paste
                .Handles<PasteAfter>(aggregate => aggregate.PasteAfter, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<PasteInto>(aggregate => aggregate.PasteInto, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Static text
                .Handles<AddStaticText>(aggregate => aggregate.AddStaticTextAndMoveIfNeeded, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<MoveStaticText>((command, aggregate) => aggregate.MoveStaticText(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateStaticText>(aggregate => aggregate.UpdateStaticText, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteStaticText>((command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>())
                // Variable
                .Handles<AddVariable>(aggregate => aggregate.AddVariableAndMoveIfNeeded, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<MoveVariable>((command, aggregate) => aggregate.MoveVariable(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateVariable>(aggregate => aggregate.UpdateVariable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteVariable>((command, aggregate) => aggregate.DeleteVariable(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>().PreProcessBy<ResourcesPreProcessor>())
                // Sharing
                .Handles<AddSharedPersonToQuestionnaire>((command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.EmailOrLogin, command.ShareType, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<RemoveSharedPersonFromQuestionnaire>((command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>());

            return Task.CompletedTask;
        }
    }
}
