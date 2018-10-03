using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.ViewModels;
using SQLite;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class EnumeratorSplashScreenAppCompatActivity<TSetup, TApplication> : MvxSplashScreenAppCompatActivity<TSetup, TApplication>
        where TSetup : MvxAppCompatSetup<TApplication>, new()
        where TApplication : class, IMvxApplication, new()
    {
        protected EnumeratorSplashScreenAppCompatActivity(int resourceId) : base(resourceId)
        {
        }

        public override Task InitializationComplete()
        {
            this.EncryptApplication();
            return base.InitializationComplete();
        }

        protected virtual void EncryptApplication() 
        {
            var settings = ServiceLocator.Current.GetInstance<IEnumeratorSettings>();
            if (!settings.Encrypted)
            {
                var encryptionService = ServiceLocator.Current.GetInstance<IEncryptionService>();

                encryptionService.GenerateKeys();

                this.EncryptDatabase(encryptionService);

                //settings.SetEncrypted(true);
            }
        }

        private void EncryptDatabase(IEncryptionService encryptionService)
        {
            var identifyingQuestionsRepository = ServiceLocator.Current.GetInstance<IPlainStorage<PrefilledQuestionView>>();
            var identifyingQuestions = identifyingQuestionsRepository.LoadAll();

            foreach (var identifyingQuestion in identifyingQuestions)
            {
                identifyingQuestion.EncryptedQuestionText = encryptionService.Encrypt(identifyingQuestion.QuestionText);
                identifyingQuestion.EncryptedAnswer = encryptionService.Encrypt(identifyingQuestion.Answer);

                identifyingQuestionsRepository.Store(identifyingQuestion);
            }

            var assignmentAnswerRepository = ServiceLocator.Current.GetInstance<IAssignmentDocumentsStorage>();
            var assignments = assignmentAnswerRepository.LoadAll();

            foreach (var assignment in assignments)
            {
                var assignmentAnswers = assignmentAnswerRepository.FetchPreloadedData(assignment).Answers;

                foreach (var assignmentAnswer in assignmentAnswers)
                {
                    assignmentAnswer.EncryptedQuestion = encryptionService.Encrypt(assignmentAnswer.Question);
                    assignmentAnswer.EncryptedAnswerAsString = encryptionService.Encrypt(assignmentAnswer.AnswerAsString);
                    assignmentAnswer.EncryptedSerializedAnswer = encryptionService.Encrypt(assignmentAnswer.SerializedAnswer);
                }

                assignmentAnswerRepository.Store(assignment);
            }

            var eventsSettings = ServiceLocator.Current.GetInstance<SqliteSettings>();
            var fileAccessor = ServiceLocator.Current.GetInstance<IFileSystemAccessor>();

            foreach (var interviewFile in fileAccessor.GetFilesInDirectory(eventsSettings.PathToInterviewsDirectory))
            {
                var sqConnection = new SQLiteConnectionString(interviewFile, true);
                var connection = new SQLiteConnectionWithLock(sqConnection,
                    openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

                using (connection.Lock())
                {
                    var events = connection.Table<EventView>().ToList();
                    foreach (var eventView in events)
                    {
                        eventView.EncryptedJsonEvent = encryptionService.Encrypt(eventView.JsonEvent);
                    }

                    connection.UpdateAll(events);
                }
            }
        }
    }
}
