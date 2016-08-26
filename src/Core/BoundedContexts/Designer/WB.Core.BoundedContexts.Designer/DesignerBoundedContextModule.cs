using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;

using AccountAR = WB.Core.BoundedContexts.Designer.Aggregates.AccountAR;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : NinjectModule
    {
        private readonly ICompilerSettings compilerSettings;

        public DesignerBoundedContextModule(ICompilerSettings compilerSettings)
        {
            this.compilerSettings = compilerSettings;
        }
        public override void Load()
        {

            this.Bind<IQuestionDetailsViewMapper>().To<QuestionDetailsViewMapper>().InSingletonScope();
            this.Bind<IQuestionnaireEntityFactory>().To<QuestionnaireEntityFactory>().InSingletonScope();
            this.Bind<IKeywordsProvider>().To<KeywordsProvider>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();

            this.Bind<IQuestionnaireListViewFactory>().To<QuestionnaireListViewFactory>();
            this.Bind<IQuestionnaireChangeHistoryFactory>().To<QuestionnaireChangeHistoryFactory>();
            this.Bind<IQuestionnaireViewFactory>().To<QuestionnaireViewFactory>();
            this.Bind<IChapterInfoViewFactory>().To<ChapterInfoViewFactory>();
            this.Bind<IQuestionnaireInfoViewFactory>().To<QuestionnaireInfoViewFactory>();
            this.Bind<IQuestionnaireSharedPersonsFactory>().To<QuestionnaireSharedPersonsFactory>();
            this.Bind<IAccountListViewFactory>().To<AccountListViewFactory>();
            this.Bind<IAccountViewFactory>().To<AccountViewFactory>();
            this.Bind<ITranslationsService>().To<TranslationsService>();
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();

            this.Unbind<IExpressionProcessor>();
            this.Bind<IExpressionProcessor>().To<RoslynExpressionProcessor>().InSingletonScope();
            this.Unbind<ICompilerSettings>();
            this.Bind<ICompilerSettings>().ToConstant(this.compilerSettings);
            this.Bind<IDynamicCompilerSettingsProvider>().To<DynamicCompilerSettingsProvider>();

            DispatcherRegistryHelper.RegisterDenormalizer<AccountDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireSharedPersonsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireListViewItemDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireChangeHistoryDenormalizer>(this.Kernel);

            this.Bind<IEventHandler>().To<QuestionnaireInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<ChaptersInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<QuestionsAndGroupsCollectionDenormalizer>().InSingletonScope();
            this.Bind<IDesignerEngineVersionService>().To<DesignerEngineVersionService>().InSingletonScope();
            this.Bind<ICodeGenerator>().To<CodeGenerator>();
            this.Bind<ILookupTableService>().To<LookupTableService>();
            this.Bind<IAttachmentService>().To<AttachmentService>();
            this.Bind(typeof(ITopologicalSorter<>)).To(typeof(TopologicalSorter<>));
            
            CommandRegistry
                .Setup<AccountAR>()
                .InitializesWith<RegisterAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.RegisterAccount(command.ApplicationName, command.UserName, command.Email, command.AccountId, command.Password, command.PasswordSalt, command.IsConfirmed, command.ConfirmationToken))
                .Handles<AddRoleToAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.AddRole(command.Role))
                .Handles<ChangeOnlineAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.ChangeOnline())
                .Handles<ChangePasswordAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.ChangePassword(command.Password))
                .Handles<ChangePasswordQuestionAndAnswerAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.ChangePasswordQuestionAndAnswer(command.PasswordQuestion, command.PasswordAnswer))
                .Handles<ChangePasswordResetTokenCommand>(command => command.AccountId, (command, aggregate) => aggregate.ChangePasswordResetToken(command.PasswordResetToken, command.PasswordResetExpirationDate))
                .Handles<ConfirmAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Confirm())
                .Handles<DeleteAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Delete())
                .Handles<LockAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Lock())
                .Handles<LoginFailedAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.LoginFailed())
                .Handles<RemoveRoleFromAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.RemoveRole(command.Role))
                .Handles<ResetPasswordAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.ResetPassword(command.Password, command.PasswordSalt))
                .Handles<UnlockAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Unlock())
                .Handles<UpdateAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Update(command.UserName, command.IsLockedOut, command.PasswordQuestion, command.Email, command.IsConfirmed, command.Comment));

            CommandRegistry
                .Setup<Questionnaire>()
                .InitializesWith<CloneQuestionnaire>(command => command.PublicKey, (command, aggregate) => aggregate.CloneQuestionnaire(command.Title, command.IsPublic, command.CreatedBy, command.PublicKey, command.Source))
                .InitializesWith<CreateQuestionnaire>(command => command.PublicKey, (command, aggregate) => aggregate.CreateQuestionnaire(command.PublicKey, command.Title, command.CreatedBy, command.IsPublic))
                .InitializesWith<ImportQuestionnaire>(command => command.QuestionnaireId, (command, aggregate) => aggregate.ImportQuestionnaire(command.CreatedBy, command.Source))
                .Handles<DeleteQuestionnaire>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteQuestionnaire())
                // Macro
                .Handles<AddMacro>(command => command.QuestionnaireId, aggregate => aggregate.AddMacro)
                .Handles<DeleteMacro>(command => command.QuestionnaireId, aggregate => aggregate.DeleteMacro)
                .Handles<UpdateMacro>(command => command.QuestionnaireId, aggregate => aggregate.UpdateMacro)
                 // LookupTable
                .Handles<AddLookupTable>(command => command.QuestionnaireId, aggregate => aggregate.AddLookupTable)
                .Handles<UpdateLookupTable>(command => command.QuestionnaireId, aggregate => aggregate.UpdateLookupTable)
                .Handles<DeleteLookupTable>(command => command.QuestionnaireId, aggregate => aggregate.DeleteLookupTable)
                // Attachment
                .Handles<AddOrUpdateAttachment>(command => command.QuestionnaireId, aggregate => aggregate.AddOrUpdateAttachment)
                .Handles<DeleteAttachment>(command => command.QuestionnaireId, aggregate => aggregate.DeleteAttachment)
                // Translation
                .Handles<AddOrUpdateTranslation>(command => command.QuestionnaireId, aggregate => aggregate.AddOrUpdateTranslation)
                .Handles<DeleteTranslation>(command => command.QuestionnaireId, aggregate => aggregate.DeleteTranslation)
                // Group
                .Handles<AddGroup>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddGroupAndMoveIfNeeded(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.Index))
                .Handles<DeleteGroup>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId))
                .Handles<MoveGroup>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Handles<UpdateGroup>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId))
                // Questions
                .Handles<MoveQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Handles<AddDefaultTypeQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddDefaultTypeQuestionAdnMoveIfNeeded(command))
                .Handles<DeleteQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteQuestion(command.QuestionId, command.ResponsibleId))
                .Handles<UpdateCascadingComboboxOptions>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Handles<UpdateDateTimeQuestion>(command => command.QuestionnaireId, aggregate => aggregate.UpdateDateTimeQuestion)
                .Handles<UpdateFilteredComboboxOptions>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Handles<UpdateGpsCoordinatesQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsPreFilled, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.ValidationConditions, command.Properties))
                .Handles<UpdateMultimediaQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateMultimediaQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Scope, command.Properties))
                .Handles<UpdateMultiOptionQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateMultiOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToEntityId, command.AreAnswersOrdered, command.MaxAllowedAnswers, command.YesNoView, command.ValidationConditions, command.LinkedFilterExpression, command.Properties))
                .Handles<UpdateNumericQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateNumericQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsPreFilled, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.Properties, command.ResponsibleId, command.IsInteger, command.CountOfDecimalPlaces, command.ValidationConditions))
                .Handles<UpdateQRBarcodeQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateQRBarcodeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Scope, command.ValidationConditions, command.Properties))
                .Handles<UpdateQuestionnaire>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId, command.HasResponsibleAdminRights))
                .Handles<UpdateSingleOptionQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateSingleOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsPreFilled, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToEntityId, command.IsFilteredCombobox, command.CascadeFromQuestionId, command.ValidationConditions, command.LinkedFilterExpression, command.Properties))
                .Handles<UpdateTextListQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateTextListQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.MaxAnswerCount, command.Scope, command.ValidationConditions, command.Properties))
                .Handles<UpdateTextQuestion>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateTextQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsPreFilled, command.Scope, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.Mask, command.ResponsibleId, command.ValidationConditions, command.Properties))                
                // Copy-Paste
                .Handles<PasteAfter>(command => command.QuestionnaireId, aggregate => aggregate.PasteAfter)
                .Handles<PasteInto>(command => command.QuestionnaireId, aggregate => aggregate.PasteInto)
                // Static text
                .Handles<AddStaticText>(command => command.QuestionnaireId, aggregate => aggregate.AddStaticTextAndMoveIfNeeded)
                .Handles<MoveStaticText>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveStaticText(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId))
                .Handles<UpdateStaticText>(command => command.QuestionnaireId, aggregate => aggregate.UpdateStaticText)
                .Handles<DeleteStaticText>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId))
                // Variable
                .Handles<AddVariable>(command => command.QuestionnaireId, aggregate => aggregate.AddVariableAndMoveIfNeeded)
                .Handles<MoveVariable>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveVariable(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId))
                .Handles<UpdateVariable>(command => command.QuestionnaireId, aggregate => aggregate.UpdateVariable)
                .Handles<DeleteVariable>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteVariable(command.EntityId, command.ResponsibleId))
                // Sharing
                .Handles<AddSharedPersonToQuestionnaire>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.Email, command.ShareType, command.ResponsibleId))
                .Handles<RemoveSharedPersonFromQuestionnaire>(command => command.QuestionnaireId, (command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId));
        }
    }
}
