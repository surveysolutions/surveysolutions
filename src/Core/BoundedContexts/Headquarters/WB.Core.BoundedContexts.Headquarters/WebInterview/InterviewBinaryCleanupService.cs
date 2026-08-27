#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Logging;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class InterviewBinaryCleanupService : IInterviewBinaryCleanupService
    {
        private readonly IPlainStorageAccessor<InterviewBinaryCleanupRecord> storage;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly ILogger<InterviewBinaryCleanupService> logger;

        public InterviewBinaryCleanupService(
            IPlainStorageAccessor<InterviewBinaryCleanupRecord> storage,
            IStatefulInterviewRepository statefulInterviewRepository,
            IImageFileStorage imageFileStorage,
            IAudioFileStorage audioFileStorage,
            ILogger<InterviewBinaryCleanupService> logger)
        {
            this.storage = storage;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
            this.logger = logger;
        }

        public void EnqueueImageCleanup(Guid interviewId, string fileName, Exception exception) =>
            Enqueue(interviewId, fileName, QuestionType.Multimedia, exception);

        public void EnqueueAudioCleanup(Guid interviewId, string fileName, Exception exception) =>
            Enqueue(interviewId, fileName, QuestionType.Audio, exception);

        public void ProcessPending(Guid interviewId)
        {
            var records = this.storage.Query(_ => _
                .Where(x => x.InterviewId == interviewId)
                .OrderBy(x => x.RequestedAt)
                .ToList());

            if (records.Count == 0)
                return;

            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());
            var activeImageFiles = GetActiveFileNames(interview, QuestionType.Multimedia);
            var activeAudioFiles = GetActiveFileNames(interview, QuestionType.Audio);

            foreach (var record in records)
            {
                var activeFiles = record.QuestionType == QuestionType.Multimedia
                    ? activeImageFiles
                    : activeAudioFiles;

                if (activeFiles.Contains(record.FileName))
                {
                    this.storage.Remove(record.Id);
                    continue;
                }

                try
                {
                    if (record.QuestionType == QuestionType.Multimedia)
                        this.imageFileStorage.RemoveInterviewBinaryData(interviewId, record.FileName).GetAwaiter().GetResult();
                    else if (record.QuestionType == QuestionType.Audio)
                        this.audioFileStorage.RemoveInterviewBinaryData(interviewId, record.FileName).GetAwaiter().GetResult();

                    this.storage.Remove(record.Id);
                }
                catch (Exception exception)
                {
                    record.FailedCount++;
                    record.RequestedAt = DateTime.UtcNow;
                    this.storage.Store(record, record.Id);
                    this.logger.LogWarning(exception,
                        "Retry cleanup for obsolete {QuestionType} file {FileName} in interview {InterviewId} failed.",
                        record.QuestionType, record.FileName, interviewId);
                }
            }
        }

        private void Enqueue(Guid interviewId, string fileName, QuestionType questionType, Exception exception)
        {
            this.logger.LogWarning(exception,
                "Failed to remove obsolete {QuestionType} file {FileName} in interview {InterviewId}. Queued for retry.",
                questionType, fileName, interviewId);

            var record = this.storage.Query(_ => _
                .FirstOrDefault(x => x.InterviewId == interviewId
                    && x.QuestionType == questionType
                    && x.FileName == fileName));

            if (record == null)
            {
                record = new InterviewBinaryCleanupRecord
                {
                    Id = Guid.NewGuid(),
                    InterviewId = interviewId,
                    FileName = fileName,
                    QuestionType = questionType
                };
            }

            record.RequestedAt = DateTime.UtcNow;
            record.FailedCount++;
            this.storage.Store(record, record.Id);
        }

        private static HashSet<string> GetActiveFileNames(IStatefulInterview? interview, QuestionType questionType)
        {
            if (interview == null)
                return new HashSet<string>(StringComparer.Ordinal);

            return interview.GetAllInterviewNodes()
                .OfType<InterviewTreeQuestion>()
                .Where(question => question.IsAnswered())
                .Select(question => questionType switch
                {
                    QuestionType.Multimedia when question.IsMultimedia => question.GetAsInterviewTreeMultimediaQuestion().GetAnswer()?.FileName,
                    QuestionType.Audio when question.IsAudio => question.GetAsInterviewTreeAudioQuestion().GetAnswer()?.FileName,
                    _ => null
                })
                .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
                .Select(fileName => fileName!)
                .ToHashSet(StringComparer.Ordinal);
        }
    }
}
