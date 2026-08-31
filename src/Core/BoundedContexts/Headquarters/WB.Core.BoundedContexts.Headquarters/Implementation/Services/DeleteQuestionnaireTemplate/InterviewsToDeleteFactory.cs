#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class InterviewsToDeleteFactory : IInterviewsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IBrokenImageFileStorage brokenImageFileStorage;
        private readonly IBrokenAudioFileStorage brokenAudioFileStorage;
        private readonly IBrokenAudioAuditFileStorage brokenAudioAuditFileStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;
        private readonly ILogger<InterviewsToDeleteFactory> logger;

        private const int BatchSize = 100;

        public InterviewsToDeleteFactory(IUnitOfWork sessionFactory, IImageFileStorage imageFileStorage,
            IAudioAuditFileStorage audioAuditFileStorage,
            IBrokenImageFileStorage brokenImageFileStorage,
            IBrokenAudioFileStorage brokenAudioFileStorage,
            IBrokenAudioAuditFileStorage brokenAudioAuditFileStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader,
            ILogger<InterviewsToDeleteFactory> logger)
        {
            this.sessionFactory = sessionFactory;
            this.imageFileStorage = imageFileStorage;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.brokenImageFileStorage = brokenImageFileStorage;
            this.brokenAudioFileStorage = brokenAudioFileStorage;
            this.brokenAudioAuditFileStorage = brokenAudioAuditFileStorage;
            this.interviewsReader = interviewsReader;
            this.logger = logger;
        }

        private async Task RemoveAllInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.sessionFactory.Session.Query<InterviewSummary>()
                .Where(s => s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version)
                .DeleteAsync();
        }

        private async Task RemoveAllEventsForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            /*await this.sessionFactory.Session.Query<RawEvent>()
                .Where(e => 
                    this.sessionFactory.Session.Query<InterviewSummary>()
                        .Any(s =>
                            s.InterviewId == e.EventSourceId
                            && s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version))
                .DeleteAsync();*/

            this.logger.LogWarning("Removing all events for {questionnaireId}", questionnaireIdentity);
            var queryText = $"DELETE FROM events as e " +
                            $"USING interviewsummaries as i " +
                            $"WHERE e.eventsourceid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.SetTimeout(300);
            await query.ExecuteUpdateAsync();
        }

        private async Task RemoveAudioAuditForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.sessionFactory.Session.Query<AudioAuditFile>()
                .Where(a => 
                    this.sessionFactory.Session.Query<InterviewSummary>()
                    .Any(s =>
                        s.InterviewId == a.InterviewId
                        && s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                        && s.QuestionnaireVersion == questionnaireIdentity.Version))
                .DeleteAsync();

            /*
            var queryText = $"DELETE FROM plainstore.audioauditfiles as a " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE a.interviewid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            await query.ExecuteUpdateAsync();
        */
        }

        private async Task RemoveAudioForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.sessionFactory.Session.Query<AudioFile>()
                .Where(af => 
                    this.sessionFactory.Session.Query<InterviewSummary>()
                        .Any(s =>
                            s.InterviewId == af.InterviewId
                            && s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version))
                .DeleteAsync();

            
            /*
            var queryText = $"DELETE FROM plainstore.audiofiles as a " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE a.interviewid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            await query.ExecuteUpdateAsync();
        */
        }

        private async Task RemoveInterviewsBinaryDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var pageIndex = 0;
            List<Guid> interviewIds;
            
            do
            {
                var skip = pageIndex * BatchSize;
                interviewIds = this.interviewsReader.Query(_ => _.Where(interview => 
                        interview.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                        interview.QuestionnaireVersion == questionnaireIdentity.Version)
                    .OrderBy(summary => summary.InterviewId)
                    .Select(summary => summary.InterviewId)
                    .Skip(skip)
                    .Take(BatchSize)
                    .ToList());

                if (interviewIds.Count == 0)
                    break;

                await RemoveBinaryDataForBatchAsync(interviewIds);

                pageIndex++;
            } while (interviewIds.Count == BatchSize);

            // Also clean up binary data for broken-only interviews (those with a BrokenInterviewPackage
            // but no InterviewSummary, which the paged summary loop above would miss).
            // BrokenInterviewPackage does not implement IReadSideRepositoryEntity, so
            // IQueryableReadSideRepositoryReader cannot be used here; direct session access is justified.
            // Page by the row's primary key (Id) to avoid DISTINCT + ORDER BY translation issues in LINQ.
            var lastBrokenId = 0;
            int brokenBatchCount;

            do
            {
                var capturedLastId = lastBrokenId;
                var batch = this.sessionFactory.Session.Query<BrokenInterviewPackage>()
                    .Where(p => p.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                                && p.QuestionnaireVersion == questionnaireIdentity.Version
                                && p.Id > capturedLastId)
                    .OrderBy(p => p.Id)
                    .Select(p => new { p.Id, p.InterviewId })
                    .Take(BatchSize)
                    .ToList();

                brokenBatchCount = batch.Count;
                if (brokenBatchCount == 0)
                    break;

                lastBrokenId = batch[brokenBatchCount - 1].Id;

                // Deduplicate in memory; multiple packages can reference the same interview.
                var brokenBatchIds = batch.Select(b => b.InterviewId).Distinct().ToList();

                // Exclude IDs already covered by the InterviewSummary loop above.
                var summaryIds = this.interviewsReader.Query(_ => _.Where(s =>
                        brokenBatchIds.Contains(s.InterviewId))
                    .Select(s => s.InterviewId)
                    .ToList());

                var onlyBrokenIds = brokenBatchIds.Except(summaryIds).ToList();
                if (onlyBrokenIds.Count > 0)
                    await RemoveBinaryDataForBatchAsync(onlyBrokenIds);
            } while (brokenBatchCount == BatchSize);
        }

        private async Task RemoveBinaryDataForBatchAsync(List<Guid> interviewIds)
        {
            await imageFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);
            await audioAuditFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);
            await brokenImageFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);
            await brokenAudioFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);
            await brokenAudioAuditFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);
        }

        public async Task RemoveAllInterviewsDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await logger.LogExecuteTimeAsync(() => RemoveInterviewsBinaryDataAsync(questionnaireIdentity), "removing interview's binary data");
            await logger.LogExecuteTimeAsync(() => RemoveAudioForInterviewsAsync(questionnaireIdentity), "removing interview's audio");
            await logger.LogExecuteTimeAsync(() => RemoveAudioAuditForInterviewsAsync(questionnaireIdentity), "removing interview's audio audit");
            await logger.LogExecuteTimeAsync(() => RemoveAllEventsForInterviewsAsync(questionnaireIdentity), "removing interview's events");
            await logger.LogExecuteTimeAsync(() => RemoveAllInterviewsAsync(questionnaireIdentity),"removing interviews");
        }
    }
}
