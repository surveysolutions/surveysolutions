using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    internal class InvitationsDeletionService : IInvitationsDeletionService
    {
        private readonly IUnitOfWork sessionFactory;

        public InvitationsDeletionService(IUnitOfWork sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public void Delete(QuestionnaireIdentity questionnaireIdentity)
        {
            var queryText = $"DELETE FROM invitations as i " +
                            $"USING assignments as a " +
                            $"WHERE i.assignmentid = a.id " +
                            $"  AND a.questionnaireid = :questionnaireId " +
                            $"  AND a.questionnaireversion = :questionnaireVersion ";

            var query = sessionFactory.Session.CreateSQLQuery(queryText);
            query.SetParameter("questionnaireId", questionnaireIdentity.QuestionnaireId);
            query.SetParameter("questionnaireVersion", questionnaireIdentity.Version);
            query.ExecuteUpdate();
        }
    }

    public interface IInvitationsDeletionService
    {
        void Delete(QuestionnaireIdentity questionnaireIdentity);
    }
}
