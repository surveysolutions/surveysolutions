using NHibernate;
using Ninject;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmetnsDeletionService : IAssignmetnsDeletionService
    {
        private readonly IUnitOfWork sessionFactory;

        public AssignmetnsDeletionService(IUnitOfWork sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Delete(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE from {nameof(Assignment)} a " +
                            $"WHERE a.QuestionnaireId.QuestionnaireId = :questionnaireId " +
                            $"AND a.QuestionnaireId.Version =  :questionnaireVersion";
            var query = sessionFactory.Session.CreateQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);

            query.ExecuteUpdate();
        }
    }
}
