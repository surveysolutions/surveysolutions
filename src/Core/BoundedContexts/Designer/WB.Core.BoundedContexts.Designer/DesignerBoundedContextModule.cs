﻿using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.CodeGenerationV3;
using WB.Core.BoundedContexts.Designer.CodeGenerationV3.Impl;
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
using WB.Core.BoundedContexts.Designer.Services.TopologicalSorter;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.Aggregates;
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
            this.Bind<IEventTypeResolver>().ToConstant(
                new EventTypeResolver(
                    typeof(DesignerBoundedContextModule).Assembly));

            this.Bind<IKeywordsProvider>().To<KeywordsProvider>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();

            this.Bind<IPlainAggregateRootRepository<User>>().To<UserRepository>();
            this.Bind<IPlainAggregateRootRepository<Questionnaire>>().To<QuestionnaireRepository>();
            this.Bind<IFindReplaceService>().ToMethod((c) => new FindReplaceService(c.Kernel.Get<IPlainAggregateRootRepository<Questionnaire>>()));

            this.Bind<IQuestionnaireListViewFactory>().To<QuestionnaireListViewFactory>();
            this.Bind<IPublicFoldersStorage>().To<PublicFoldersStorage>();
            this.Bind<IQuestionnaireChangeHistoryFactory>().To<QuestionnaireChangeHistoryFactory>();
            this.Bind<IQuestionnaireViewFactory>().To<QuestionnaireViewFactory>();
            this.Bind<IChapterInfoViewFactory>().To<ChapterInfoViewFactory>();
            this.Bind<IQuestionnaireInfoViewFactory>().To<QuestionnaireInfoViewFactory>();
            this.Bind<IAccountListViewFactory>().To<AccountListViewFactory>();
            this.Bind<IAccountViewFactory>().To<AccountViewFactory>();
            this.Bind<IAllowedAddressService>().To<AllowedAddressService>();
            this.Bind<IQuestionnaireCompilationVersionService>().To<QuestionnaireCompilationVersionService>();
            this.Bind<IIpAddressProvider>().To<IpAddressProvider>();
            this.Bind<ITranslationsService>().To<TranslationsService>();
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();

            this.Unbind<IExpressionProcessor>();
            this.Bind<IExpressionProcessor>().To<RoslynExpressionProcessor>().InSingletonScope();
            this.Unbind<ICompilerSettings>();
            this.Bind<ICompilerSettings>().ToConstant(this.compilerSettings);
            this.Bind<IDynamicCompilerSettingsProvider>().To<DynamicCompilerSettingsProvider>();

            this.Bind<IDesignerEngineVersionService>().To<DesignerEngineVersionService>().InSingletonScope();
            this.Bind<ICodeGenerator>().To<CodeGenerator>();
            this.Bind<ICodeGeneratorV2>().To<CodeGeneratorV2>();
            this.Bind<IQuestionTypeToCSharpTypeMapper>().To<QuestionTypeToCSharpTypeMapper>();

            this.Bind<ICodeGeneratorV3>().To<CodeGeneratorV3>();
            this.Bind<ICodeGenerationModelsFactoryV3>().To<CodeGenerationModelsFactoryV3>();
            this.Bind<IQuestionTypeToCSharpTypeMapperV3>().To<QuestionTypeToCSharpTypeMapperV3>();

            this.Bind<ICodeGenerationModelsFactory>().To<CodeGenerationModelsFactory>();
            this.Bind<ILookupTableService>().To<LookupTableService>();
            this.Bind<IAttachmentService>().To<AttachmentService>();
            this.Bind(typeof(ITopologicalSorter<>)).To(typeof(TopologicalSorter<>));
            
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
                .InitializesWith<CloneQuestionnaire>((command, aggregate) => aggregate.CloneQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId, command.QuestionnaireId, command.Source), config =>config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .InitializesWith<CreateQuestionnaire>((command, aggregate) => aggregate.CreateQuestionnaire(command.QuestionnaireId, command.Title, command.ResponsibleId, command.IsPublic), config =>config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .InitializesWith<ImportQuestionnaire>((command, aggregate) => aggregate.ImportQuestionnaire(command.ResponsibleId, command.Source), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteQuestionnaire>((command, aggregate) => aggregate.DeleteQuestionnaire(), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
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
                // Group
                .Handles<AddGroup>((command, aggregate) => aggregate.AddGroupAndMoveIfNeeded(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.Index), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteGroup>((command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<MoveGroup>((command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateGroup>((command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.HideIfDisabled, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Questions
                .Handles<MoveQuestion>((command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<AddDefaultTypeQuestion>((command, aggregate) => aggregate.AddDefaultTypeQuestionAdnMoveIfNeeded(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteQuestion>((command, aggregate) => aggregate.DeleteQuestion(command.QuestionId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateCascadingComboboxOptions>((command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<ListViewPostProcessor>())
                .Handles<UpdateDateTimeQuestion>(aggregate => aggregate.UpdateDateTimeQuestion, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateFilteredComboboxOptions>((command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options), config => config.PostProcessBy<ListViewPostProcessor>())
                .Handles<UpdateGpsCoordinatesQuestion>((command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateMultimediaQuestion>((command, aggregate) => aggregate.UpdateMultimediaQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.EnablementCondition, command.HideIfDisabled, command.Instructions, command.ResponsibleId, command.Scope, command.Properties), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
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
                .Handles<DeleteStaticText>((command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Variable
                .Handles<AddVariable>(aggregate => aggregate.AddVariableAndMoveIfNeeded, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<MoveVariable>((command, aggregate) => aggregate.MoveVariable(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<UpdateVariable>(aggregate => aggregate.UpdateVariable, config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<DeleteVariable>((command, aggregate) => aggregate.DeleteVariable(command.EntityId, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                // Sharing
                .Handles<AddSharedPersonToQuestionnaire>((command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.Email, command.ShareType, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>())
                .Handles<RemoveSharedPersonFromQuestionnaire>((command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId), config => config.PostProcessBy<ListViewPostProcessor>().PostProcessBy<HistoryPostProcessor>());
        }
    }
}
