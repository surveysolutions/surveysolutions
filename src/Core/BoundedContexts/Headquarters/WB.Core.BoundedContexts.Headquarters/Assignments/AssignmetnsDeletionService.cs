using NHibernate;
using Ninject;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmetnsDeletionService : IAssignmetnsDeletionService
    {
        private readonly ISessionFactory sessionFactory;

        public AssignmetnsDeletionService([Named(PostgresPlainStorageModule.SessionFactoryName)]ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Delete(QuestionnaireIdentity questionnaireIdentity)
        {
            using (var session = this.sessionFactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    var queryText = $"DELETE from {nameof(Assignment)} a " +
                        $"WHERE a.QuestionnaireId.QuestionnaireId = :questionnaireId " +
                        $"AND a.QuestionnaireId.Version =  :questionnaireVersion";
                    var query = session.CreateQuery(queryText);
                    query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
                    query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);

                    query.ExecuteUpdate();
                    transaction.Commit();
                }
            }
        }
    }
}