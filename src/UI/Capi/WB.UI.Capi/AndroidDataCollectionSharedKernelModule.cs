using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;
using WB.UI.Shared.Android.Services;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class AndroidDataCollectionSharedKernelModule : NinjectModule
    {
        private readonly string basePath;
        private readonly string syncDirectoryName;

        public AndroidDataCollectionSharedKernelModule(string basePath, string syncDirectoryName = "SYNC",
            string questionnaireAssembliesFolder = "QuestionnaireAssemblies")
        {
            this.basePath = basePath;
            this.syncDirectoryName = syncDirectoryName;
        }

        public override void Load()
        {
            this.Bind<IQuestionnaireFactory>().To<QuestionnaireFactory>();

            this.Bind<IQuestionnaireRosterStructureFactory>().To<QuestionnaireRosterStructureFactory>();

            this.Bind<IInterviewSynchronizationFileStorage>()
                .To<InterviewSynchronizationFileStorage>().InSingletonScope().WithConstructorArgument("rootDirectoryPath", this.basePath).WithConstructorArgument("syncDirectoryName", this.syncDirectoryName);

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