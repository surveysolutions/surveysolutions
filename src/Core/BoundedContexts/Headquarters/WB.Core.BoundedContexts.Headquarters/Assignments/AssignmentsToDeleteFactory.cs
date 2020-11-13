#nullable enable

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    class AssignmentsToDeleteFactory : IAssignmentsToDeleteFactory
    {
        private readonly IUnitOfWork sessionFactory;
        private readonly IWorkspaceNameProvider workspaceNameProvider;
        private readonly ILogger<AssignmentsToDeleteFactory> logger;

        public AssignmentsToDeleteFactory(IUnitOfWork sessionFactory,
            IWorkspaceNameProvider workspaceNameProvider,
            ILogger<AssignmentsToDeleteFactory> logger)
        {
            this.sessionFactory = sessionFactory;
            this.workspaceNameProvider = workspaceNameProvider;
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

            
            var queryText = $"DELETE FROM {this.workspaceNameProvider.CurrentWorkspace()}.events as e " +
                            $"USING {this.workspaceNameProvider.CurrentWorkspace()}.assignments as a " +
                            $"WHERE e.eventsourceid = a.publickey " +
                            $"  AND a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            await query.ExecuteUpdateAsync();
        }

        public async Task RemoveAllAssignmentsDataAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            await logger.LogExecuteTimeAsync(() => RemoveAllEventsForAssignmentsAsync(questionnaireIdentity), "removing assignment's events");
            await logger.LogExecuteTimeAsync(() => RemoveAllAssignmentsAsync(questionnaireIdentity), "removing assignments");
        }
    }
}
