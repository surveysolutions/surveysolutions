#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing.Storage;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
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
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ILogger logger;

        private const int BatchSize = 100;

        public InterviewsToDeleteFactory(IUnitOfWork sessionFactory, IImageFileStorage imageFileStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader,
            IQuestionnaireStorage questionnaireStorage,
            ILogger logger)
        {
            this.sessionFactory = sessionFactory;
            this.imageFileStorage = imageFileStorage;
            this.interviewsReader = interviewsReader;
            this.questionnaireStorage = questionnaireStorage;
            this.logger = logger;
        }

        private async Task RemoveAllInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing interview summaries");

            await this.sessionFactory.Session.Query<InterviewSummary>()
                .Where(s => s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version)
                .DeleteAsync();
            
            logger.LogInformation($"Finished removing interview summaries. Elapsed time: {stopwatch.Elapsed}");
        }

        private async Task RemoveAllEventsForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing interview's events");

            await this.sessionFactory.Session.Query<RawEvent>()
                .Where(e => 
                    this.sessionFactory.Session.Query<InterviewSummary>()
                        .Any(s =>
                            s.InterviewId == e.EventSourceId
                            && s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && s.QuestionnaireVersion == questionnaireIdentity.Version))
                .DeleteAsync();

            /*
            var queryText = $"DELETE FROM events.events as e " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE e.eventsourceid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
            */

            logger.LogInformation($"Finished removing interview's events. Elapsed time: {stopwatch.Elapsed}");
        }

        private async Task RemoveAudioAuditForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing interview's audio audit");

            await this.sessionFactory.Session.Query<AudioAuditFile>()
                .Where(a => 
                    this.sessionFactory.Session.Query<InterviewSummary>()
                    .Any(s =>
                        s.InterviewId == a.InterviewId
                        && s.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                        && s.QuestionnaireVersion == questionnaireIdentity.Version))
                .DeleteAsync();

            /*var queryText = $"DELETE FROM plainstore.audioauditfiles as a " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE a.interviewid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();*/

            logger.LogInformation($"Finished removing interview's audio audit. Elapsed time: {stopwatch.Elapsed}");
        }

        private async Task RemoveAudioForInterviewsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing interview's audio");

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
            query.ExecuteUpdate();
            */

            logger.LogInformation($"Finished removing interview's audio. Elapsed time: {stopwatch.Elapsed}");
        }

        private async Task RemoveInterviewsImagesAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
            if (questionnaire == null)
                return;
            
            var hasImageQuestions = questionnaire.Find<IMultimediaQuestion>().Any();
            
            if (!hasImageQuestions)
                return;
            
            var pageIndex = 0;
            List<Guid> interviewIds;
            
            do
            {
                var skip = pageIndex * BatchSize;
                interviewIds = this.interviewsReader.Query(_ => _.Where(interview => 
                        interview.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                        interview.QuestionnaireVersion == questionnaireIdentity.Version)
                    .Select(summary => summary.InterviewId)
                    .Skip(skip)
                    .Take(BatchSize)
                    .ToList());

                await imageFileStorage.RemoveAllBinaryDataForInterviewsAsync(interviewIds);

                pageIndex++;
            } while (interviewIds.Count > 0 && interviewIds.Count == BatchSize);
            
            logger.LogInformation($"Finished removing interview's images. Elapsed time: {stopwatch.Elapsed}");
        }

        public async Task RemoveAllInterviewsDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await logger.LogExecuteTimeAsync(() => RemoveInterviewsImagesAsync(questionnaireIdentity), "removing interview's images");
            await logger.LogExecuteTimeAsync(() => RemoveAudioForInterviewsAsync(questionnaireIdentity), "removing interview's audio");
            await logger.LogExecuteTimeAsync(() => RemoveAudioAuditForInterviewsAsync(questionnaireIdentity), "removing interview's audio audit");
            await logger.LogExecuteTimeAsync(() => RemoveAllEventsForInterviewsAsync(questionnaireIdentity), "removing interview's events");
            await logger.LogExecuteTimeAsync(() => RemoveAllInterviewsAsync(questionnaireIdentity),"removing interviews");
        }
    }
}
