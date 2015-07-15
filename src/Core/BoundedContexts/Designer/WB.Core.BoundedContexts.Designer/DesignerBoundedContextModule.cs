using System;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
using AccountAR = WB.Core.BoundedContexts.Designer.Aggregates.AccountAR;
using Questionnaire = WB.Core.BoundedContexts.Designer.Aggregates.Questionnaire;

namespace WB.Core.BoundedContexts.Designer
{
    public class DesignerBoundedContextModule : NinjectModule
    {
        private readonly IDynamicCompilerSettings dynamicCompilerSettings;

        public DesignerBoundedContextModule(IDynamicCompilerSettings dynamicCompilerSettings)
        {
            this.dynamicCompilerSettings = dynamicCompilerSettings;
        }
        public override void Load()
        {

            this.Bind<IQuestionDetailsViewMapper>().To<QuestionDetailsViewMapper>().InSingletonScope();
            this.Bind<IQuestionnaireEntityFactory>().To<QuestionnaireEntityFactory>().InSingletonScope();
            this.Bind<IKeywordsProvider>().To<KeywordsProvider>();
            this.Bind<ISubstitutionService>().To<SubstitutionService>();
            this.Bind<IQuestionnaireListViewFactory>().To<QuestionnaireListViewFactory>();
            this.Bind<IQuestionnaireChangeHistoryFactory>().To<QuestionnaireChangeHistoryFactory>();

            this.Bind<IAsyncExecutor>().To<AsyncExecutor>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Unbind<IExpressionProcessor>();
            this.Bind<IExpressionProcessor>().To<RoslynExpressionProcessor>().InSingletonScope();
            this.Unbind<IDynamicCompilerSettings>();
            this.Bind<IDynamicCompilerSettings>().ToConstant(this.dynamicCompilerSettings);

            DispatcherRegistryHelper.RegisterDenormalizer<AccountDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireSharedPersonsDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireListViewItemDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<PdfQuestionnaireDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<QuestionnaireChangeHistoryDenormalizer>(this.Kernel);

            this.Bind<IEventHandler>().To<QuestionnaireInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<ChaptersInfoViewDenormalizer>().InSingletonScope();
            this.Bind<IEventHandler>().To<QuestionsAndGroupsCollectionDenormalizer>().InSingletonScope();
            this.Bind<IExpressionsEngineVersionService>().To<ExpressionsEngineVersionService>().InSingletonScope();
            this.Bind<ICodeGenerator>().To<CodeGenerator>();

            this.Kernel.RegisterFactory<QuestionnaireListViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireViewFactory>();
            this.Kernel.RegisterFactory<ChapterInfoViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireInfoViewFactory>();
            this.Kernel.RegisterFactory<QuestionnaireSharedPersonsFactory>();
            this.Kernel.RegisterFactory<AccountListViewFactory>();
            this.Kernel.RegisterFactory<AccountViewFactory>();
            this.Kernel.RegisterFactory<PdfQuestionnaireFactory>();

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
                .Handles<UpdateAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Update(command.UserName, command.IsLockedOut, command.PasswordQuestion, command.Email, command.IsConfirmed, command.Comment))
                .Handles<ValidateAccountCommand>(command => command.AccountId, (command, aggregate) => aggregate.Validate());

            CommandRegistry
                .Setup<Questionnaire>()
                .InitializesWith<CloneQuestionnaireCommand>(command => command.PublicKey, (command, aggregate) => aggregate.CloneQuestionnaire(command.Title, command.IsPublic, command.CreatedBy, command.PublicKey, command.Source))
                .InitializesWith<CreateQuestionnaireCommand>(command => command.PublicKey, (command, aggregate) => aggregate.CreateQuestionnaire(command.PublicKey, command.Title, command.CreatedBy, command.IsPublic))
                .InitializesWith<ImportQuestionnaireCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.ImportQuestionnaire(command.CreatedBy, command.Source))
                .Handles<AddGroupCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddGroupAndMoveIfNeeded(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId, command.Index))
                .Handles<AddSharedPersonToQuestionnaireCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.Email, command.ShareType, command.ResponsibleId))
                .Handles<AddStaticTextCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddStaticTextAndMoveIfNeeded(command.EntityId, command.ParentId, command.Text, command.ResponsibleId, command.Index))                
                .Handles<CloneGroupCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.CloneGroup(command.GroupId, command.ResponsibleId, command.SourceGroupId, command.TargetIndex))
                .Handles<CloneGroupWithoutChildrenCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.CloneGroupWithoutChildren(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.ParentGroupId, command.SourceGroupId, command.TargetIndex, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId))
                .Handles<CloneQuestionByIdCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.CloneQuestionById(command.QuestionId, command.ResponsibleId, command.TargetId))
                .Handles<CloneStaticTextCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.CloneStaticText(command.EntityId, command.SourceEntityId, command.ResponsibleId))
                .Handles<DeleteGroupCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId))
                .Handles<DeleteQuestionnaireCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteQuestionnaire())
                .Handles<DeleteStaticTextCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId))
                .Handles<MoveGroupCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Handles<MoveQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Handles<MoveStaticTextCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.MoveStaticText(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId))
                .Handles<AddDefaultTypeQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.AddDefaultTypeQuestionAdnMoveIfNeeded(command))
                .Handles<DeleteQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.DeleteQuestion(command.QuestionId, command.ResponsibleId))
                .Handles<RemoveSharedPersonFromQuestionnaireCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId))
                .Handles<UpdateCascadingComboboxOptionsCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Handles<UpdateDateTimeQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateDateTimeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId))
                .Handles<UpdateFilteredComboboxOptionsCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Handles<UpdateGpsCoordinatesQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId))
                .Handles<UpdateGroupCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.IsRoster, command.RosterSizeSource, command.FixedRosterTitles, command.RosterTitleQuestionId))
                .Handles<UpdateMultimediaQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateMultimediaQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Handles<UpdateMultiOptionQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateMultiOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers))
                .Handles<UpdateNumericQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateNumericQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.IsInteger, command.CountOfDecimalPlaces))
                .Handles<UpdateQRBarcodeQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateQRBarcodeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId))
                .Handles<UpdateQuestionnaireCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId))
                .Handles<UpdateSingleOptionQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateSingleOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Handles<UpdateStaticTextCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateStaticText(command.EntityId, command.Text, command.ResponsibleId))
                .Handles<UpdateTextListQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateTextListQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.MaxAnswerCount))
                .Handles<UpdateTextQuestionCommand>(command => command.QuestionnaireId, (command, aggregate) => aggregate.UpdateTextQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Mask, command.ResponsibleId));
        }
    }
}
