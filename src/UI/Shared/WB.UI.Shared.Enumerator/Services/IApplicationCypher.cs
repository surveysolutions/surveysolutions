using System.Linq;
using SQLite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Shared.Enumerator.Services
{
    public interface IApplicationCypher
    {
        void EncryptAppData();
    }

    class ApplicationCypher : IApplicationCypher
    {
        private readonly IEnumeratorSettings settings;
        private readonly IAssignmentDocumentsStorage assignmentDocumentsStorage;
        private readonly IEncryptionService encryptionService;
        private readonly IPlainStorage<AudioFileMetadataView> audioViewStorage;
        private readonly IPlainStorage<InterviewMultimediaView> multimediaView;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly SqliteSettings databaseSettings;
        private readonly IPlainStorage<AudioFileView> fileViewStorage;
        private readonly IPlainStorage<InterviewFileView> interviewFileView;
        private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestions;

        public ApplicationCypher(IEnumeratorSettings enumeratorSettings,
            IAssignmentDocumentsStorage assignmentDocumentsStorage,
            IEncryptionService encryptionService,
            IPlainStorage<AudioFileMetadataView> audioViewStorage,
            IPlainStorage<AudioFileView> fileViewStorage, 
            IPlainStorage<InterviewFileView> interviewFileView,
            IPlainStorage<InterviewMultimediaView> multimediaView,
            IFileSystemAccessor fileSystemAccessor,
            SqliteSettings databaseSettings, IPlainStorage<PrefilledQuestionView> identifyingQuestions)
        {
            this.settings = enumeratorSettings;
            this.assignmentDocumentsStorage = assignmentDocumentsStorage;
            this.encryptionService = encryptionService;
            this.audioViewStorage = audioViewStorage;
            this.fileViewStorage = fileViewStorage;
            this.interviewFileView = interviewFileView;
            this.multimediaView = multimediaView;
            this.fileSystemAccessor = fileSystemAccessor;
            this.databaseSettings = databaseSettings;
            this.identifyingQuestions = identifyingQuestions;
        }

        public void EncryptAppData()
        {
            if (settings.Encrypted) return;

            encryptionService.GenerateKeys();

            EncryptIdentifyingQuestions();
            EncryptAssignments();
            EncryptEvents();
            EncryptMultimedia();

            settings.SetEncrypted(true);
        }

        private void EncryptMultimedia()
        {
            EncryptPictures();
            EncryptAudio();
        }

        private void EncryptAudio()
        {
            foreach (var audioInfo in audioViewStorage.LoadAll())
            {
                var audioFile = fileViewStorage.GetById(audioInfo.FileId);

                audioFile.File = encryptionService.Encrypt(audioFile.File);
                fileViewStorage.Store(audioFile);
            }
        }

        private void EncryptPictures()
        {
            foreach (var imageInfo in multimediaView.LoadAll())
            {
                var imageFile = interviewFileView.GetById(imageInfo.FileId);

                imageFile.File = encryptionService.Encrypt(imageFile.File);
                interviewFileView.Store(imageFile);
            }
        }

        private void EncryptEvents()
        {
            foreach (var interviewFile in this.fileSystemAccessor.GetFilesInDirectory(databaseSettings.PathToInterviewsDirectory, "*.sqlite3"))
            {
                var sqConnection = new SQLiteConnectionString(interviewFile, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true, null);
                var connection = new SQLiteConnectionWithLock(sqConnection);

                using (connection.Lock())
                {
                    connection.CreateTable<EventView>();

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

        private void EncryptAssignments()
        {
            var assignmentAnswerRepository = this.assignmentDocumentsStorage;
            var assignments = assignmentAnswerRepository.LoadAll();

            foreach (var assignment in assignments)
            {
                var assignmentAnswers = assignmentAnswerRepository.FetchPreloadedData(assignment).Answers;
                assignment.Answers = assignmentAnswers;
                assignmentAnswerRepository.Store(assignment);
            }
        }

        private void EncryptIdentifyingQuestions()
        {
            var stored = this.identifyingQuestions.Where(x =>
                (x.Answer != null && x.EncryptedAnswer == null) ||
                (x.QuestionText != null && x.EncryptedQuestionText == null));

            this.identifyingQuestions.Store(stored);
        }
    }
}
