using System;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class QuestionnaireSearchStorage : IQuestionnaireSearchStorage
    {
        public const string TableName = "questionnairesearchentities";
        public const string TableNameWithSchema = "plainstore." + TableName;

        private readonly IUnitOfWork unitOfWork;

        public QuestionnaireSearchStorage(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public void AddOrUpdateEntity(Guid questionnaireId, IComposite composite)
        {
            var isQuestion = composite is IQuestion;
            if (!isQuestion)
                throw new ArgumentException("composite type is not supported " + composite.GetType());

            var sql = $"INSERT INTO {TableNameWithSchema} s (questionnaireid, entityid, entity, searchtext)" +
                      $"VALUES(:questionnaireId, :entityid, :entity, :searchtext) " +
                      $"ON CONFLICT (questionnaireid, entityid) DO UPDATE " +
                      $"SET questionnaireid = :questionnaireId," +
                      $"    entityid        = :entityId," +
                      $"    entity          = :entity," +
                      $"    searchtext      = :searchText";

            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("questionnaireId", questionnaireId);
            query.SetParameter("entityId", composite.PublicKey);
            query.SetParameter("entity", composite);
            query.SetParameter("searchText", GetTextUsedForSerach(composite));
            query.ExecuteUpdate();
        }

        private string GetTextUsedForSerach(IComposite composite)
        {
            return composite.GetTitle();
        }

        public void Remove(Guid questionnaireId, Guid entityId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = :questionnaireId " +
                      $"AND s.entityid =  :entityId";
            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("questionnaireId", questionnaireId);
            query.SetParameter("entityId", entityId);
            query.ExecuteUpdate();
        }

        public void RemoveAllEntities(Guid questionnaireId)
        {
            var sql = $"DELETE from {TableNameWithSchema} s " +
                      $"WHERE s.questionnaireid = :questionnaireId ";
            var query = unitOfWork.Session.CreateQuery(sql);
            query.SetParameter("questionnaireId", questionnaireId);
            query.ExecuteUpdate();
        }

        public SearchResult Search(SearchInput input)
        {
            return null;
        }
    }

    public interface IQuestionnaireSearchStorage
    {
        void AddOrUpdateEntity(Guid questionnaireId, IComposite composite);
        void Remove(Guid questionnaireId, Guid entityId);
        SearchResult Search(SearchInput input);
        void RemoveAllEntities(Guid questionnaireId);
    }
}
