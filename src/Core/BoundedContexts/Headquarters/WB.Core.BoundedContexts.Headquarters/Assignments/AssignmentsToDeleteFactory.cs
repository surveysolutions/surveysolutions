#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ncqrs.Eventing.Storage;
using NHibernate.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentsToDeleteFactory : IAssignmentsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;
        private readonly ILogger logger;

        public AssignmentsToDeleteFactory(IUnitOfWork sessionFactory,
            ILogger logger)
        {
            this.sessionFactory = sessionFactory;
            this.logger = logger;
        }

        private async Task RemoveAllAssignmentsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing assignments");

            await this.sessionFactory.Session.Query<Assignment>()
                .Where(a => a.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && a.QuestionnaireId.Version == questionnaireIdentity.Version)
                .DeleteAsync();

            logger.LogInformation($"Finished removing assignments. Elapsed time: {stopwatch.Elapsed}");
        }

        private async Task RemoveAllEventsForAssignmentsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            logger.LogInformation("Start removing assignment's events");

            await this.sessionFactory.Session.Query<RawEvent>()
                .Where(e => 
                    this.sessionFactory.Session.Query<Assignment>()
                        .Any(a =>
                            a.PublicKey == e.EventSourceId
                            && a.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && a.QuestionnaireId.Version == questionnaireIdentity.Version))
                .DeleteAsync();

            /*
            var queryText = $"DELETE FROM events.events as e " +
                            $"USING readside.assignments as a " +
                            $"WHERE e.eventsourceid = a.publickey " +
                            $"  AND a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        */
                        
            logger.LogInformation($"Finished removing assignment's events. Elapsed time: {stopwatch.Elapsed}");
        }

        public async Task RemoveAllAssignmentsDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await logger.LogExecuteTimeAsync(() => RemoveAllEventsForAssignmentsAsync(questionnaireIdentity), "removing assignment's events");
            await logger.LogExecuteTimeAsync(() => RemoveAllAssignmentsAsync(questionnaireIdentity), "removing assignments");
        }
    }
}
