using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.DateTime;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.GpsCoordinates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Mulimedia;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.MultiOption;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Numeric;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.QRBarcode;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.SingleOption;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Text;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
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
using WB.Core.Infrastructure.CommandBus;
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

            CommandRegistry.For<Aggregates.Questionnaire>()
                .Add<AddDateTimeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddDateTimeQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId))
                .Add<AddGpsCoordinatesQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddGpsCoordinatesQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Add<AddGroupCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.ParentGroupId, command.IsRoster, command.RosterSizeSource, command.RosterFixedTitles, command.RosterTitleQuestionId))
                .Add<AddMultiOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddMultiOptionQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers))
                .Add<AddNumericQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddNumericQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.MaxValue, command.ResponsibleId, command.IsInteger, command.CountOfDecimalPlaces))
                .Add<AddQRBarcodeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddQRBarcodeQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Add<AddQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.NewAddQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.Type, command.VariableName, command.VariableLabel, command.Mask, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Options, command.ResponsibleId, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<AddSharedPersonToQuestionnaireCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddSharedPerson(command.PersonId, command.Email, command.ResponsibleId))
                .Add<AddSingleOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddSingleOptionQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Options, command.LinkedToQuestionId, command.ResponsibleId, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<AddStaticTextCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddStaticText(command.EntityId, command.ParentId, command.Text, command.ResponsibleId))
                .Add<AddTextListQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddTextListQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId, command.MaxAnswerCount))
                .Add<AddTextQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.AddTextQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Mask, command.ResponsibleId))
                .Add<CloneDateTimeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneDateTimeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ParentGroupId, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId))
                .Add<CloneGpsCoordinatesQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneGpsCoordinatesQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ParentGroupId, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId))
                .Add<CloneGroupCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneGroup(command.GroupId, command.ResponsibleId, command.SourceGroupId, command.TargetIndex))
                .Add<CloneGroupWithoutChildrenCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneGroupWithoutChildren(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.ParentGroupId, command.SourceGroupId, command.TargetIndex, command.IsRoster, command.RosterSizeSource, command.RosterFixedTitles, command.RosterTitleQuestionId))
                .Add<CloneMultiOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneMultiOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ParentGroupId, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers))
                .Add<CloneNumericQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneNumericQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId, command.MaxValue, command.IsInteger, command.CountOfDecimalPlaces))
                .Add<CloneQRBarcodeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneQRBarcodeQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId))
                .Add<CloneQuestionByIdCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneQuestionById(command.QuestionId, command.ResponsibleId, command.TargetId))
                .Add<CloneQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.Type, command.VariableName, command.VariableLabel, command.Mask, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Options, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<CloneSingleOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneSingleOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ParentGroupId, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<CloneStaticTextCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneStaticText(command.EntityId, command.SourceEntityId, command.ResponsibleId))
                .Add<CloneTextListQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneTextListQuestion(command.QuestionId, command.ParentGroupId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId, command.MaxAnswerCount))
                .Add<CloneTextQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.CloneTextQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Mask, command.ParentGroupId, command.SourceQuestionId, command.TargetIndex, command.ResponsibleId))
                .Add<DeleteGroupCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.DeleteGroup(command.GroupId, command.ResponsibleId))
                .Add<DeleteQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.NewDeleteQuestion(command.QuestionId, command.ResponsibleId))
                .Add<DeleteQuestionnaireCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.DeleteQuestionnaire())
                .Add<DeleteStaticTextCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.DeleteStaticText(command.EntityId, command.ResponsibleId))
                .Add<MigrateExpressionsToCSharp>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.MigrateExpressionsToCSharp())
                .Add<MoveGroupCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.MoveGroup(command.GroupId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Add<MoveQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.MoveQuestion(command.QuestionId, command.TargetGroupId, command.TargetIndex, command.ResponsibleId))
                .Add<MoveStaticTextCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.MoveStaticText(command.EntityId, command.TargetEntityId, command.TargetIndex, command.ResponsibleId))
                .Add<RemoveSharedPersonFromQuestionnaireCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.RemoveSharedPerson(command.PersonId, command.Email, command.ResponsibleId))
                .Add<UpdateCascadingComboboxOptionsCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateCascadingComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Add<UpdateDateTimeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateDateTimeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId))
                .Add<UpdateFilteredComboboxOptionsCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateFilteredComboboxOptions(command.QuestionId, command.ResponsibleId, command.Options))
                .Add<UpdateGpsCoordinatesQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateGpsCoordinatesQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Add<UpdateGroupCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateGroup(command.GroupId, command.ResponsibleId, command.Title, command.VariableName, command.RosterSizeQuestionId, command.Description, command.Condition, command.IsRoster, command.RosterSizeSource, command.RosterFixedTitles, command.RosterTitleQuestionId))
                .Add<UpdateMultimediaQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateMultimediaQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Add<UpdateMultiOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateMultiOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers))
                .Add<UpdateNumericQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateNumericQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.MaxValue, command.ResponsibleId, command.IsInteger, command.CountOfDecimalPlaces))
                .Add<UpdateQRBarcodeQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateQRBarcodeQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId))
                .Add<UpdateQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateQuestion(command.QuestionId, command.Title, command.Type, command.VariableName, command.VariableLabel, command.Mask, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Options, command.ResponsibleId, command.LinkedToQuestionId, command.AreAnswersOrdered, command.MaxAllowedAnswers, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<UpdateQuestionnaireCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateQuestionnaire(command.Title, command.IsPublic, command.ResponsibleId))
                .Add<UpdateSingleOptionQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateSingleOptionQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.ResponsibleId, command.Options, command.LinkedToQuestionId, command.IsFilteredCombobox, command.CascadeFromQuestionId))
                .Add<UpdateStaticTextCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateStaticText(command.EntityId, command.Text, command.ResponsibleId))
                .Add<UpdateTextListQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateTextListQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.EnablementCondition, command.Instructions, command.ResponsibleId, command.MaxAnswerCount))
                .Add<UpdateTextQuestionCommand>(
                    command => command.QuestionnaireId,
                    (command, aggregate) => aggregate.UpdateTextQuestion(command.QuestionId, command.Title, command.VariableName, command.VariableLabel, command.IsMandatory, command.IsPreFilled, command.Scope, command.EnablementCondition, command.ValidationExpression, command.ValidationMessage, command.Instructions, command.Mask, command.ResponsibleId));

            CommandRegistry.For<Aggregates.AccountAR>()
                .Add<AddRoleToAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.AddRole(command.Role))
                .Add<ChangeOnlineAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.ChangeOnline())
                .Add<ChangePasswordAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.ChangePassword(command.Password))
                .Add<ChangePasswordQuestionAndAnswerAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.ChangePasswordQuestionAndAnswer(command.PasswordQuestion, command.PasswordAnswer))
                .Add<ChangePasswordResetTokenCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.ChangePasswordResetToken(command.PasswordResetToken, command.PasswordResetExpirationDate))
                .Add<ConfirmAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Confirm())
                .Add<DeleteAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Delete())
                .Add<LockAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Lock())
                .Add<LoginFailedAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.LoginFailed())
                .Add<RemoveRoleFromAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.RemoveRole(command.Role))
                .Add<ResetPasswordAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.ResetPassword(command.Password, command.PasswordSalt))
                .Add<UnlockAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Unlock())
                .Add<UpdateAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Update(command.UserName, command.IsLockedOut, command.PasswordQuestion, command.Email, command.IsConfirmed, command.Comment))
                .Add<ValidateAccountCommand>(
                    command => command.AccountId,
                    (command, aggregate) => aggregate.Validate());
        }
    }
}
