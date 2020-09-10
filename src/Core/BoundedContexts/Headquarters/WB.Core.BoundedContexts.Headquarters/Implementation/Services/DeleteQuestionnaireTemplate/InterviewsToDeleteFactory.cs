using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class InterviewsToDeleteFactory : IInterviewsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;

        public const int BatchSize = 100;

        public InterviewsToDeleteFactory(IUnitOfWork sessionFactory)
        {
            this.sessionFactory = sessionFactory;
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
    }
}
