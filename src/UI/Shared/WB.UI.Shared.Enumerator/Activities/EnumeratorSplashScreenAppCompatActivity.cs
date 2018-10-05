using System.Diagnostics;
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

                EncryptIdentifyingQuestions();
                EncryptAssignments();
                EncryptEvents(encryptionService);

                settings.SetEncrypted(true);
            }
        }

        private static void EncryptEvents(IEncryptionService encryptionService)
        {
            var eventsSettings = ServiceLocator.Current.GetInstance<SqliteSettings>();
            var fileAccessor = ServiceLocator.Current.GetInstance<IFileSystemAccessor>();

            foreach (var interviewFile in fileAccessor.GetFilesInDirectory(eventsSettings.PathToInterviewsDirectory))
            {
                var sqConnection = new SQLiteConnectionString(interviewFile, true);
                var connection = new SQLiteConnectionWithLock(sqConnection,
                    openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);

                using (connection.Lock())
                {
                    var events = connection.Table<EventView>().Where(x => x.EncryptedJsonEvent == null).ToList();
                    foreach (var eventView in events)
                    {
                        eventView.EncryptedJsonEvent = encryptionService.Encrypt(eventView.JsonEvent);
                        eventView.JsonEvent = null;
                    }

                    if (events.Any()) connection.UpdateAll(events);
                }
            }
        }

        private static void EncryptAssignments()
        {
            var assignmentAnswerRepository = ServiceLocator.Current.GetInstance<IAssignmentDocumentsStorage>();
            var assignments = assignmentAnswerRepository.LoadAll();

            foreach (var assignment in assignments)
            {
                var assignmentAnswers = assignmentAnswerRepository.FetchPreloadedData(assignment).Answers;
                assignment.Answers = assignmentAnswers;
                assignmentAnswerRepository.Store(assignment);
            }
        }

        private static void EncryptIdentifyingQuestions()
        {
            var identifyingQuestionsRepository = ServiceLocator.Current.GetInstance<IPlainStorage<PrefilledQuestionView>>();
            var identifyingQuestions = identifyingQuestionsRepository.Where(x =>
                (x.Answer != null && x.EncryptedAnswer == null) ||
                (x.QuestionText != null && x.EncryptedQuestionText == null));

            identifyingQuestionsRepository.Store(identifyingQuestions);
        }
    }
}
