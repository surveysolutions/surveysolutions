using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
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

        private const int BatchSize = 100;

        public InterviewsToDeleteFactory(IUnitOfWork sessionFactory, IImageFileStorage imageFileStorage,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewsReader,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.sessionFactory = sessionFactory;
            this.imageFileStorage = imageFileStorage;
            this.interviewsReader = interviewsReader;
            this.questionnaireStorage = questionnaireStorage;
        }

        public void RemoveAllInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM readside.interviewsummaries as i " +
                            $"WHERE i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public void RemoveAllEventsForInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM events.events as e " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE e.eventsourceid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public void RemoveAudioAuditForInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM plainstore.audioauditfiles as a " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE a.interviewid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public void RemoveAudioForInterviews(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM plainstore.audiofiles as a " +
                            $"USING readside.interviewsummaries as i " +
                            $"WHERE a.interviewid = i.interviewid " +
                            $"  AND i.questionnaireid = :questionnaireId " +
                            $"  AND i.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public async Task RemoveInterviewsImagesAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaire = questionnaireStorage.GetQuestionnaireDocument(questionnaireIdentity);
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
        }
    }
}
