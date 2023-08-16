#nullable enable

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentsToDeleteFactory : IAssignmentsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;
        private readonly ILogger<AssignmentsToDeleteFactory> logger;

        public AssignmentsToDeleteFactory(IUnitOfWork sessionFactory,
            ILogger<AssignmentsToDeleteFactory> logger)
        {
            this.sessionFactory = sessionFactory;
            this.logger = logger;
        }

        private async Task RemoveAllAssignmentsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await this.sessionFactory.Session.Query<Assignment>()
                .Where(a => a.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && a.QuestionnaireId.Version == questionnaireIdentity.Version)
                .DeleteAsync();
        }

        private async Task RemoveAllEventsForAssignmentsAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            /*await this.sessionFactory.Session.Query<RawEvent>()
                .Where(e => 
                    this.sessionFactory.Session.Query<Assignment>()
                        .Any(a =>
                            a.PublicKey == e.EventSourceId
                            && a.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId
                            && a.QuestionnaireId.Version == questionnaireIdentity.Version))
                .DeleteAsync();*/

            
            var queryText = $"DELETE FROM events as e " +
                            $"USING assignments as a " +
                            $"WHERE e.eventsourceid = a.publickey " +
                            $"  AND a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.SetTimeout(300);
            await query.ExecuteUpdateAsync();
        }

        public async Task RemoveAllAssignmentsDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await logger.LogExecuteTimeAsync(() => RemoveAllEventsForAssignmentsAsync(questionnaireIdentity), "removing assignment's events");
            await logger.LogExecuteTimeAsync(() => RemoveAllAssignmentsAsync(questionnaireIdentity), "removing assignments");
        }
    }
}
