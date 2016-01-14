using Ninject.Modules;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class AndroidDataCollectionSharedKernelModule : NinjectModule
    {
        public override void Load()
        {
            CommandRegistry
                .Setup<Questionnaire>()
                .ResolvesIdFrom<QuestionnaireCommand>           (command => command.QuestionnaireId)
                .InitializesWith<ImportFromDesigner>            (aggregate => aggregate.ImportFromDesigner)
                .InitializesWith<ImportFromDesignerForTester>   (aggregate => aggregate.ImportFromDesignerForTester)
                .InitializesWith<ImportFromSupervisor>          (aggregate => aggregate.ImportFromSupervisor)
                .InitializesWith<RegisterPlainQuestionnaire>    (aggregate => aggregate.RegisterPlainQuestionnaire)
                .Handles<DeleteQuestionnaire>                   (aggregate => aggregate.DeleteQuestionnaire)
                .Handles<DisableQuestionnaire>(aggregate => aggregate.DisableQuestionnaire);

            CommandRegistry
                .Setup<User>()
                .InitializesWith<CreateUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.CreateUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.Password, command.PublicKey, command.Roles, command.Supervisor, command.UserName, command.PersonName, command.PhoneNumber))
                .Handles<ChangeUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.ChangeUser(command.Email, command.IsLockedBySupervisor, command.IsLockedByHQ, command.PasswordHash, command.PersonName, command.PhoneNumber, command.UserId))
                .Handles<LockUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.Lock())
                .Handles<LockUserBySupervisorCommand>(command => command.UserId, (command, aggregate) => aggregate.LockBySupervisor())
                .Handles<UnlockUserCommand>(command => command.PublicKey, (command, aggregate) => aggregate.Unlock())
                .Handles<UnlockUserBySupervisorCommand>(command => command.PublicKey, (command, aggregate) => aggregate.UnlockBySupervisor());
        }
    }
}