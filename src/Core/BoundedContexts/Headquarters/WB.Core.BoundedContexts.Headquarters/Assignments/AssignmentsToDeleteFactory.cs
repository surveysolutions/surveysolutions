using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentsToDeleteFactory : IAssignmentsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;

        public const int BatchSize = 100;

        public AssignmentsToDeleteFactory(IUnitOfWork sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void RemoveAllAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM readside.assignments as a " +
                            $"WHERE a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public void RemoveAllEventsForAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM events.events as e " +
                            $"USING readside.assignments as a " +
                            $"WHERE e.eventsourceid = a.publickey " +
                            $"  AND a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }

        public void RemoveAllAssignmentsData(QuestionnaireIdentity questionnaireIdentity)
        {
            RemoveAllEventsForAssignments(questionnaireIdentity);
            RemoveAllAssignments(questionnaireIdentity);
        }
    }
}
