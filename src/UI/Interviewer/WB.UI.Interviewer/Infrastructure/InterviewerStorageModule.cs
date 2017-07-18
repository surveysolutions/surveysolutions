using System.IO;
using Main.Core.Documents;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Services.Synchronization;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerStorageModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<QuestionnaireKeyValueStorage>().InSingletonScope();

            this.Bind<IInterviewerQuestionnaireAccessor>().To<InterviewerQuestionnaireAccessor>();
            this.Bind<IInterviewerInterviewAccessor>().To<InterviewerInterviewAccessor>();
            this.Bind<IInterviewEventStreamOptimizer>().To<InterviewEventStreamOptimizer>();
            this.Bind<IQuestionnaireTranslator>().To<QuestionnaireTranslator>();
            this.Bind<IQuestionnaireStorage>().To<QuestionnaireStorage>().InSingletonScope();
            this.Bind<IAudioFileStorage>().To<InterviewerAudioFileStorage>();
            this.Bind<IImageFileStorage>().To<InterviewerImageFileStorage>();
            this.Bind<IAnswerToStringConverter>().To<AnswerToStringConverter>();
            this.Bind<IAssignmentDocumentsStorage>().To<AssignmentDocumentsStorage>().InSingletonScope();

            this.Bind<IInterviewerEventStorage, IEventStore>()
                .To<SqliteMultiFilesEventStorage>()
                .InSingletonScope();

            this.Bind<SqliteSettings>().ToConstant(
                new SqliteSettings
                {
                    PathToDatabaseDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory("data"),
                    PathToInterviewsDirectory = AndroidPathUtils.GetPathToSubfolderInLocalDirectory($"data{Path.DirectorySeparatorChar}interviews")
                });

            this.Bind(typeof(IPlainStorage<,>)).To(typeof(SqlitePlainStorage<,>)).InSingletonScope();
            this.Bind(typeof(IPlainStorage<>)).To(typeof(SqlitePlainStorage<>)).InSingletonScope();
        }
    }
}